type ColorScheme = "light" | "dark";

interface BootSnapshot {
    version: string;
    language: string;
    storage: string;
    rootVariables: string;
    html: string;
}

interface LanguageInUse {
    code: string;
    localeFolder: string | null;
}

const STORAGE_KEY_PREFIX = "colorpal-";
const THEME_STORAGE_KEY = "colorpal-theme";
const LANGUAGE_STORAGE_KEY = "colorpal-language";
const BOOT_SNAPSHOT_STORAGE_KEY = "colorpal-boot-snapshot";
const BOOT_SNAPSHOT_ELEMENT_ID = "bootSnapshot";
const BOOT_SNAPSHOT_SAVE_DELAY_MS = 300;
const BOOT_VIEW_HOLD_TIMEOUT_MS = 500;
const HEIGHT_TOLERANCE_PX = 1;
const PRIMARY_COLOR_VARIABLE = "--primary-color";
const DARK_PRIMARY_COLOR = "var(--dark-primary-color)";
const LANGUAGE_CODE_MESSAGE = "languageCode";
const AUTOMATIC_LANGUAGE = "automatic";
const DEFAULT_LANGUAGE_CODE = "en";
const LANGUAGE_CODE_PATTERN = /^[a-z]{2,3}(-[A-Z]{2}|-\d{3})?$/;
const RIGHT_TO_LEFT = "rtl";
const RIGHT_TO_LEFT_LANGUAGES: ReadonlySet<string> = new Set(["ar", "fa", "he"]);
const LANGUAGES_CHROME_SKIPS: ReadonlySet<string> = new Set(["nb"]);
const LOCALE_FOLDER_EXCEPTION_MAP: Readonly<Record<string, string>> = { nb: "no" };

let bootSnapshotSaveTimer: number | undefined;
let storageAtLastHomeChange: string | null = null;
let isWatchingBootSnapshot = false;

// Gets client color scheme
function getClientColorScheme(): ColorScheme {
    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

function getStoredValue(storageKey: string): unknown {
    const storedValue = localStorage.getItem(storageKey);

    if (storedValue === null) {
        return null;
    }

    try {
        return JSON.parse(storedValue);
    } catch {
        return storedValue;
    }
}

function getStoredTheme(): ColorScheme | null {
    const theme = getStoredValue(THEME_STORAGE_KEY);

    return theme === "light" || theme === "dark" ? theme : null;
}

function getStoredLanguage(): string {
    const language = getStoredValue(LANGUAGE_STORAGE_KEY);

    return typeof language === "string" && LANGUAGE_CODE_PATTERN.test(language) ? language : AUTOMATIC_LANGUAGE;
}

function getExtensionVersion(): string | null {
    return globalThis.chrome?.runtime?.id === undefined ? null : chrome.runtime.getManifest().version;
}

function getLocaleFolder(languageCode: string): string {
    return LOCALE_FOLDER_EXCEPTION_MAP[languageCode] ?? languageCode.replace("-", "_");
}

function resolveLanguage(language: string): LanguageInUse {
    if (language !== AUTOMATIC_LANGUAGE) {
        return { code: language, localeFolder: getLocaleFolder(language) };
    }

    const i18n = globalThis.chrome?.i18n;

    if (i18n === undefined) {
        return { code: DEFAULT_LANGUAGE_CODE, localeFolder: getLocaleFolder(DEFAULT_LANGUAGE_CODE) };
    }

    const chromeLanguage = i18n.getUILanguage();

    if (LANGUAGES_CHROME_SKIPS.has(chromeLanguage)) {
        return { code: chromeLanguage, localeFolder: getLocaleFolder(chromeLanguage) };
    }

    return { code: i18n.getMessage(LANGUAGE_CODE_MESSAGE) || DEFAULT_LANGUAGE_CODE, localeFolder: null };
}

function applyLanguage(languageCode: string): void {
    const root = document.documentElement;
    root.lang = languageCode;

    if (RIGHT_TO_LEFT_LANGUAGES.has(languageCode.split("-")[0])) {
        root.dir = RIGHT_TO_LEFT;
    } else {
        root.removeAttribute("dir");
    }
}

function getStorageFingerprint(): string {
    const entries: string[] = [];

    for (let index = 0; index < localStorage.length; index++) {
        const key = localStorage.key(index);

        if (key !== null && key.startsWith(STORAGE_KEY_PREFIX) && key !== BOOT_SNAPSHOT_STORAGE_KEY) {
            entries.push(`${key}=${localStorage.getItem(key)}`);
        }
    }

    return entries.sort().join("\n");
}

function readBootSnapshot(version: string, language: string): BootSnapshot | null {
    const storedValue = localStorage.getItem(BOOT_SNAPSHOT_STORAGE_KEY);

    if (storedValue === null) {
        return null;
    }

    let snapshot: Partial<BootSnapshot>;

    try {
        snapshot = JSON.parse(storedValue);
    } catch (error) {
        console.warn("Ignoring the stored boot snapshot because it is not valid JSON.", error);
        return null;
    }

    const isCurrent = typeof snapshot.html === "string" && typeof snapshot.rootVariables === "string"
        && snapshot.version === version && snapshot.language === language && snapshot.storage === getStorageFingerprint();

    return isCurrent ? snapshot as BootSnapshot : null;
}

function showBootSnapshot(snapshot: BootSnapshot): void {
    const container = document.createElement("div");
    container.id = BOOT_SNAPSHOT_ELEMENT_ID;
    container.className = "boot-snapshot";
    container.inert = true;
    container.setAttribute("aria-hidden", "true");
    container.innerHTML = snapshot.html;

    document.documentElement.setAttribute("style", snapshot.rootVariables);
    document.documentElement.append(container);
}

function getRootVariables(): string {
    const rootStyle = document.documentElement.style;

    return Array.from(rootStyle)
        .filter((name) => name.startsWith("--"))
        .map((name) => `${name}: ${rootStyle.getPropertyValue(name)}`)
        .join("; ");
}

function holdBootViewHeight(height: number): void {
    const rootStyle = document.documentElement.style;
    rootStyle.minHeight = `${height}px`;

    const release = (): void => {
        resizeObserver.disconnect();
        window.clearTimeout(releaseTimer);
        rootStyle.removeProperty("min-height");
    };
    const resizeObserver = new ResizeObserver(() => {
        if (document.body.getBoundingClientRect().height >= height - HEIGHT_TOLERANCE_PX) {
            release();
        }
    });
    const releaseTimer = window.setTimeout(release, BOOT_VIEW_HOLD_TIMEOUT_MS);
    resizeObserver.observe(document.body);
}

function applyBootView(): void {
    const version = getExtensionVersion();
    const snapshot = version === null ? null : readBootSnapshot(version, document.documentElement.lang);

    if (snapshot !== null) {
        showBootSnapshot(snapshot);
        return;
    }

    if ((getStoredTheme() ?? getClientColorScheme()) === "dark") {
        document.documentElement.style.setProperty(PRIMARY_COLOR_VARIABLE, DARK_PRIMARY_COLOR);
    }
}

function isHomeAtRest(): boolean {
    return document.getElementById("colorsGrid") !== null
        && document.querySelector("#colorToolsBar [aria-pressed='true']") === null
        && !document.body.classList.contains("hidden")
        && document.body.style.display !== "none";
}

function copyHomeForSnapshot(app: HTMLElement): HTMLElement {
    const copy = app.cloneNode(true) as HTMLElement;
    copy.querySelector(".selected-color__name")?.remove();
    copy.querySelector(".rating-reminder")?.remove();

    const liveColorsPerLine = document.getElementById("colorsPerLine");
    const copiedColorsPerLine = copy.querySelector("#colorsPerLine");

    if (liveColorsPerLine instanceof HTMLSelectElement && copiedColorsPerLine instanceof HTMLSelectElement) {
        for (const option of copiedColorsPerLine.options) {
            option.toggleAttribute("selected", option.value === liveColorsPerLine.value);
        }
    }

    return copy;
}

function saveBootSnapshot(): void {
    bootSnapshotSaveTimer = undefined;
    const version = getExtensionVersion();
    const app = document.getElementById("app");
    const storage = getStorageFingerprint();

    if (version === null || app === null || !isHomeAtRest() || storage !== storageAtLastHomeChange) {
        return;
    }

    const snapshot: BootSnapshot = {
        version,
        language: document.documentElement.lang,
        storage,
        rootVariables: getRootVariables(),
        html: copyHomeForSnapshot(app).innerHTML,
    };

    try {
        localStorage.setItem(BOOT_SNAPSHOT_STORAGE_KEY, JSON.stringify(snapshot));
    } catch (error) {
        console.warn("Saving the boot snapshot failed.", error);
    }
}

function scheduleBootSnapshotSave(): void {
    storageAtLastHomeChange = getStorageFingerprint();
    window.clearTimeout(bootSnapshotSaveTimer);
    bootSnapshotSaveTimer = window.setTimeout(saveBootSnapshot, BOOT_SNAPSHOT_SAVE_DELAY_MS);
}

function flushBootSnapshotSave(): void {
    if (bootSnapshotSaveTimer !== undefined) {
        window.clearTimeout(bootSnapshotSaveTimer);
        saveBootSnapshot();
    }
}

function watchBootSnapshot(): void {
    const app = document.getElementById("app");

    if (isWatchingBootSnapshot || app === null || getExtensionVersion() === null) {
        return;
    }

    isWatchingBootSnapshot = true;
    new MutationObserver(scheduleBootSnapshotSave).observe(app, { subtree: true, childList: true, attributes: true, characterData: true });
    new MutationObserver(scheduleBootSnapshotSave).observe(document.documentElement, { attributes: true, attributeFilter: ["style"] });
    window.addEventListener("pagehide", flushBootSnapshotSave);
    document.addEventListener("visibilitychange", () => {
        if (document.visibilityState === "hidden") {
            flushBootSnapshotSave();
        }
    });
    scheduleBootSnapshotSave();
}

function completeBootView(): void {
    const snapshotElement = document.getElementById(BOOT_SNAPSHOT_ELEMENT_ID);

    if (snapshotElement !== null) {
        holdBootViewHeight(snapshotElement.getBoundingClientRect().height);
        snapshotElement.remove();
    }

    watchBootSnapshot();
}

applyLanguage(resolveLanguage(getStoredLanguage()).code);
applyBootView();
