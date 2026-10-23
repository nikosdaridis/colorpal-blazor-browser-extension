var stateServiceReference: any;
var colorsGridComponent: any;
var draggingElement: HTMLElement | null = null;
var replacingElement: HTMLElement | null = null;
var mouseOverColor: boolean = false;

// Initializes state service reference
function initializeStateService(dotNetObjectReference: any) {
    stateServiceReference = dotNetObjectReference;
}

// Initializes colors grid component reference
function initializeColorsGridComponent(dotNetObjectReference: any) {
    colorsGridComponent = dotNetObjectReference;
}

function disposeColorsGridComponent(): void {
    colorsGridComponent = null;
}

// Shows body after initialization
function isInitialized(): void {
    completeBootView();
    document.querySelector("body")?.classList.remove("hidden");
}

// Gets dark or light theme filter 
function getThemeFilter(theme: string): string {
    const themeType = theme === "dark" ? "light" : "dark";
    const filterValue = getComputedStyle(document.documentElement).getPropertyValue(`--${themeType}-theme-filter`).trim();
    return filterValue;
};

// Gets version from manifest.json
async function getManifestVersionAsync(): Promise<string> {
    const response = await fetch("manifest.json");
    const manifest = await response.json();
    return manifest.version;
}

function getLocalStorageItem(key: string): string | null {
    return localStorage.getItem(key);
}

function setLocalStorageItem(key: string, value: string): void {
    localStorage.setItem(key, value);
}

// Opens external URL in new tab, chrome.tabs gives the tab no window.opener into the privileged popup
async function openExternalUrl(url: string): Promise<void> {
    if (globalThis.chrome?.tabs !== undefined) {
        await chrome.tabs.create({ url });
        return;
    }

    window.open(url, "_blank", "noopener,noreferrer");
}

// Sets color picker position based on color tools bar position
function setColorPickerPosition() {
    const colorToolsBar = document.getElementById("colorToolsBar");
    const colorPicker = document.getElementById("colorPicker");

    if (colorToolsBar && colorPicker) {
        colorPicker.style.top = `${colorToolsBar.getBoundingClientRect().top}px`;
    }
}

// Sets value of select element
function setSelectValue(elementId: string, value: string): void {
    const selectElement = document.getElementById(elementId) as HTMLSelectElement;
    if (!selectElement) {
        return;
    }

    selectElement.value = value;
}

// Sets value of grid columns
function setGridColumns(elementId: string, columns: string): void {
    const gridElement = document.getElementById(elementId) as HTMLElement;
    if (!gridElement) {
        return;
    }

    gridElement.style.gridTemplateColumns = `repeat(${columns}, 1fr)`;
}

function isFocusLost(): boolean {
    return document.activeElement === null || document.activeElement === document.body;
}

function restoreLostFocus(selector: string): void {
    if (!isFocusLost()) {
        return;
    }

    document.querySelector<HTMLElement>(selector)?.focus();
}

// Activates eye dropper and returns hex color
function activateEyeDropper(): Promise<string | null> {
    const focusedElement = document.activeElement;
    document.body.style.display = "none";

    return new Promise((resolve) => {
        setTimeout(async function () {
            try {
                const eyeDropper = new (window as any).EyeDropper();
                const { sRGBHex } = await eyeDropper.open();

                resolve(sRGBHex);
            } catch {
                resolve(null);
            } finally {
                document.body.style.display = "block";

                if (isFocusLost() && focusedElement instanceof HTMLElement) {
                    focusedElement.focus();
                }
            }
        }, 100);
    });
}

// Opens color picker
function openColorPicker(elementId: string): void {
    const colorInput = document.getElementById(elementId);
    colorInput?.click();
}

// Copies text to clipboard
async function copyToClipboard(text: string): Promise<void> {
    if (!document.hasFocus())
        window.focus();

    try {
        await navigator.clipboard.writeText(text);
    } catch (error) {
        console.error("Copying to the clipboard failed.", error);
    }
}

// Downloads Png image of saved colors
async function downloadPng(savedColors: string[], colorsPerLine: number, showColorNames: boolean, addHexCharacter: boolean): Promise<void> {
    if (!savedColors.length)
        return;

    const cardWidth = 200;
    const cardHeight = 250;
    const columns = Math.min(colorsPerLine, savedColors.length);

    try {
        const canvas = await drawColors();
        const blob = await new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, "image/png"));

        if (blob === null) {
            throw new Error("The canvas could not be encoded as a PNG.");
        }

        const blobUrl = URL.createObjectURL(blob);
        const link = window.document.createElement("a");
        link.href = blobUrl;
        link.download = "ColorPal-Palette.png";
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    } catch (error) {
        console.error("Exporting the PNG failed.", error);
    }

    async function drawColors(): Promise<HTMLCanvasElement> {
        const fontFamily = "Poppins, sans-serif";
        const lineHeightRatio = 1.5;
        const nameFontSizeThresholds: [number, number][] = [[33, 10], [30, 11], [25, 12], [20, 13], [15, 15], [10, 16]];
        const defaultNameFontSize = 18;
        const codesFontSize = 16;
        const codesLineHeight = 22;
        const codesMarginTop = 15;
        const indexFontSize = 12;
        const indexLineHeight = indexFontSize * lineHeightRatio;
        const indexMarginTop = 5;
        const indexLineTop = cardHeight - indexLineHeight;

        await document.fonts.load(`${codesFontSize}px ${fontFamily}`);

        const canvas = document.createElement("canvas");
        canvas.width = columns * cardWidth;
        canvas.height = Math.ceil(savedColors.length / columns) * cardHeight;

        const context = canvas.getContext("2d");

        if (context === null) {
            throw new Error("The canvas has no 2D context.");
        }

        const fillLine = (text: string, x: number, lineTop: number, lineHeight: number): void => {
            const { fontBoundingBoxAscent, fontBoundingBoxDescent } = context.measureText(text);
            const halfLeading = Math.floor((lineHeight - fontBoundingBoxAscent - fontBoundingBoxDescent) / 2);
            context.fillText(text, x, lineTop + halfLeading + fontBoundingBoxAscent);
        };

        for (const [index, color] of savedColors.entries()) {
            const cardLeft = (index % columns) * cardWidth;
            const cardTop = Math.floor(index / columns) * cardHeight;
            const cardCenter = cardLeft + cardWidth / 2;
            const rgbColor = hexToRgb(color);
            const colorCodes = [addHexCharacter ? color : color.slice(1), hexToRgb(color, "string"), rgbToHsl(rgbColor, "string"), rgbToHsv(rgbColor, "string")];
            const codesLineTop = indexLineTop - indexMarginTop - colorCodes.length * codesLineHeight;

            context.fillStyle = color;
            context.fillRect(cardLeft, cardTop, cardWidth, cardHeight);

            context.save();
            context.fillStyle = "white";
            context.shadowColor = "black";
            context.shadowBlur = 4;

            if (showColorNames) {
                const name = await findClosestColorName(rgbColor);
                const nameFontSize = nameFontSizeThresholds.find(([threshold]) => name.length > threshold)?.[1] ?? defaultNameFontSize;
                const nameLineHeight = nameFontSize * lineHeightRatio;

                context.font = `${nameFontSize}px ${fontFamily}`;
                context.textAlign = "center";
                fillLine(name, cardCenter, cardTop + codesLineTop - codesMarginTop - nameLineHeight, nameLineHeight);
            }

            context.font = `${codesFontSize}px ${fontFamily}`;
            context.textAlign = "left";
            const codesWidth = Math.max(...colorCodes.map((colorCode) => context.measureText(colorCode).width));
            colorCodes.forEach((colorCode, line) => fillLine(colorCode, cardCenter - codesWidth / 2, cardTop + codesLineTop + line * codesLineHeight, codesLineHeight));

            context.font = `${indexFontSize}px ${fontFamily}`;
            context.textAlign = "center";
            fillLine(String(index + 1), cardCenter, cardTop + indexLineTop, indexLineHeight);

            context.restore();
        }

        return canvas;

        async function findClosestColorName(rgb: ColorRGB): Promise<string> {
            try {
                return await stateServiceReference?.invokeMethodAsync("FindClosestRoundedColorName", rgb) ?? "";
            } catch {
                return "";
            }
        }
    }
}

// Downloads CSV file of saved colors
function downloadCsv(dataString: string, fileName: string): void {
    const link = document.createElement("a");
    link.href = `data:text/csv;charset=utf-8,${encodeURIComponent(dataString)}`;
    link.download = `${fileName}.csv`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Handles colors move tool listeners
function handleColorsMove(): void {
    const colorsGrid = document.getElementById("colorsGrid");

    colorsGrid?.addEventListener("dragstart", onDragStart);
    colorsGrid?.addEventListener("dragend", onDragEnd);
    colorsGrid?.addEventListener("dragover", onDragOver);
    colorsGrid?.addEventListener("dragenter", onDragEnter);
    colorsGrid?.addEventListener("dragleave", onDragLeave);
    colorsGrid?.addEventListener("drop", onDrop);
}

// Removes colors move tool listeners
function removeMoveColorsListeners() {
    const colorsGrid = document.getElementById("colorsGrid");

    colorsGrid?.removeEventListener("dragstart", onDragStart);
    colorsGrid?.removeEventListener("dragend", onDragEnd);
    colorsGrid?.removeEventListener("dragover", onDragOver);
    colorsGrid?.removeEventListener("dragenter", onDragEnter);
    colorsGrid?.removeEventListener("dragleave", onDragLeave);
    colorsGrid?.removeEventListener("drop", onDrop);
};

function findColorRectangle(event: Event): HTMLElement | null {
    return event.target instanceof Element ? event.target.closest<HTMLElement>("[data-color]") : null;
}

function onDragStart(event: Event) {
    draggingElement = findColorRectangle(event);
    draggingElement?.classList.add("dragging");
}

function onDragEnd() {
    draggingElement?.classList.remove("dragging");
    replacingElement?.classList.remove("replacing");
    draggingElement = null;
    replacingElement = null;
}

function onDragOver(event: Event) {
    const colorRectangle = findColorRectangle(event);

    if (colorRectangle === null) {
        return;
    }

    event.preventDefault();
    replacingElement = colorRectangle;
    replacingElement.classList.add("replacing");
}

function onDragEnter(event: Event) {
    mouseOverColor = true;

    if (findColorRectangle(event) !== null) {
        event.preventDefault();
    }
}

function onDragLeave(event: Event) {
    const colorRectangle = findColorRectangle(event);

    if (colorRectangle === null) {
        return;
    }

    mouseOverColor = false;
    colorRectangle.classList.remove("replacing");
}

function onDrop(event: Event) {
    const colorRectangle = findColorRectangle(event);

    if (colorRectangle === null || draggingElement === null) {
        return;
    }

    event.preventDefault();

    if (draggingElement.dataset.color !== colorRectangle.dataset.color) {
        swapColors(draggingElement, colorRectangle);
    }
}

// Swaps saved colors, saves to local storage and renders colors grid
function swapColors(drag: HTMLElement, replace: HTMLElement) {
    const savedColors = JSON.parse(localStorage.getItem("colorpal-saved-colors-array") || "[]");
    const [dragIndex, replaceIndex] = [savedColors.indexOf(drag.dataset.color), savedColors.indexOf(replace.dataset.color)];

    if (dragIndex === -1 || replaceIndex === -1)
        return;

    [savedColors[dragIndex], savedColors[replaceIndex]] = [replace.dataset.color, drag.dataset.color];

    localStorage.setItem("colorpal-saved-colors-array", JSON.stringify(savedColors));
    colorsGridComponent?.invokeMethodAsync("RenderSavedColorsAsync", null);
}
