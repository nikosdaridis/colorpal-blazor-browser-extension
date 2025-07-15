declare const domtoimage: any;

var stateServiceReference: any;
var colorsGridComponent: any;
var draggingElement: HTMLElement | null = null;
var replacingElement: HTMLElement | null = null;
var mouseOverColor = false;

// Initializes state service reference
function initializeStateService(dotNetObjectReference: any) {
    stateServiceReference = dotNetObjectReference;
}

// Initializes colors grid component reference
function initializeColorsGridComponent(dotNetObjectReference: any) {
    colorsGridComponent = dotNetObjectReference;
}

// Shows body after initialization
function isInitialized(): void {
    document.querySelector("body")?.classList.remove("hidden");
}

// Gets client color scheme
function getClientColorScheme(): "light" | "dark" {
    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
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

// Sets color picker position based on color tools bar position
function setColorPickerPosition() {
    const colorToolsBar = document.getElementById("colorToolsBar");
    const colorPicker = document.getElementById("colorPicker");

    if (colorToolsBar && colorPicker)
        colorPicker.style.top = `${colorToolsBar.getBoundingClientRect().top}px`;
}

// Sets value of select element
function setSelectValue(elementId: string, value: string): void {
    const selectElement = document.getElementById(elementId) as HTMLSelectElement;
    if (!selectElement)
        return;

    selectElement.value = value;
}

// Sets value of grid columns
function setGridColumns(elementId: string, columns: string): void {
    const gridElement = document.getElementById(elementId) as HTMLElement;
    if (!gridElement)
        return;

    gridElement.style.gridTemplateColumns = `repeat(${columns}, 1fr)`;
}

// Activates eye dropper and returns hex color
function activateEyeDropper(): Promise<string | null> {
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
function copyToClipboard(text: string): void {
    if (!document.hasFocus())
        window.focus();

    try {
        navigator.clipboard.writeText(text);
    }
    catch { }
}

// Sets value of element by id
function setElementValueById(elementId: string, value: string): void {
    const element = document.getElementById(elementId);
    if (!element)
        return;

    element.textContent = value;
}

// Downloads Png image of saved colors
async function downloadPng(savedColors: string[], colorsPerLine: number, showColorNames: boolean, addHexCharacter: boolean): Promise<void> {
    if (!savedColors.length)
        return;

    const cardWidth = 200;
    const cardHeight = 250;
    const columns = savedColors.length < 3 ? 3 : Math.min(colorsPerLine, savedColors.length);

    const colorsContainer = document.createElement("div");
    colorsContainer.append(await drawColors());
    const node = document.body.appendChild(colorsContainer);

    domtoimage
        .toBlob(node)
        .then((blob: Blob) => {
            const blobUrl = URL.createObjectURL(blob);
            const link = window.document.createElement("a");
            link.href = blobUrl;
            link.download = "ColorPal-Palette.png";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        })
        .catch()
        .finally(() => node.remove());

    async function drawColors(): Promise<HTMLElement> {
        const colorsContainer = document.createElement("div");
        colorsContainer.style.display = "grid";
        colorsContainer.style.gridTemplateColumns = `repeat(${colorsPerLine}, 1fr)`;

        const colorRectTemplate = document.createElement("div");
        colorRectTemplate.style.display = "grid";
        colorRectTemplate.style.gridTemplateRows = "1fr";
        colorRectTemplate.style.justifyItems = "center";
        colorRectTemplate.style.alignItems = "end";
        colorRectTemplate.style.width = `${cardWidth}px`;
        colorRectTemplate.style.height = `${cardHeight}px`;
        colorRectTemplate.style.color = "white";
        colorRectTemplate.style.textShadow = "0px 0px 4px black";

        const nameTextTemplate = document.createElement("h1");
        nameTextTemplate.style.whiteSpace = "nowrap";

        const colorsTextTemplate = document.createElement("h2");
        colorsTextTemplate.style.display = "grid";
        colorsTextTemplate.style.marginTop = "15px";
        colorsTextTemplate.style.fontSize = "16px";
        colorsTextTemplate.style.lineHeight = "22px";
        colorsTextTemplate.style.whiteSpace = "pre";

        const indexTextTemplate = document.createElement("h2");
        indexTextTemplate.style.marginTop = "5px";
        indexTextTemplate.style.fontSize = "12px";

        let index = 1;
        for (const color of savedColors) {
            const colorRect = colorRectTemplate.cloneNode(false) as HTMLElement;
            colorRect.style.backgroundColor = color;

            const rgbColor = hexToRgb(color);

            if (showColorNames) {
                const nameText = nameTextTemplate.cloneNode(false) as HTMLElement;
                nameText.textContent = await findClosestColorName(rgbColor) as string;

                const nameLength = nameText.textContent.length;
                if (nameLength > 33) nameText.style.fontSize = "10px";
                else if (nameLength > 30) nameText.style.fontSize = "11px";
                else if (nameLength > 25) nameText.style.fontSize = "12px";
                else if (nameLength > 20) nameText.style.fontSize = "13px";
                else if (nameLength > 15) nameText.style.fontSize = "15px";
                else if (nameLength > 10) nameText.style.fontSize = "16px";
                else nameText.style.fontSize = "18px";

                colorRect.appendChild(nameText);
            }

            const colorsText = colorsTextTemplate.cloneNode(false) as HTMLElement;

            colorsText.textContent += `${addHexCharacter ? color : color.slice(1)}\r\n`;
            colorsText.textContent += `${hexToRgb(color, "string")}\r\n`;
            colorsText.textContent += `${rgbToHsl(rgbColor, "string")}\r\n`;
            colorsText.textContent += `${rgbToHsv(rgbColor, "string")}`;

            const indexText = indexTextTemplate.cloneNode(false) as HTMLElement;
            indexText.textContent = String(index++);

            colorRect.appendChild(colorsText);
            colorRect.appendChild(indexText);
            colorsContainer.appendChild(colorRect);
        }

        return colorsContainer;

        async function findClosestColorName(rgb: ColorRGB): Promise<string> {
            try {
                return await stateServiceReference?.invokeMethodAsync("FindClosestRoundedColorName", rgb) as string;
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
    const colors = document.querySelectorAll<HTMLElement>("[id*='colorRectangle']");

    colors.forEach(addMoveColorsListeners);
}

// Adds colors move tool listeners
function addMoveColorsListeners(draggable: HTMLElement) {
    draggable.addEventListener("dragstart", onDragStart);
    draggable.addEventListener("dragend", onDragEnd);
    draggable.addEventListener("dragover", onDragOver);
    draggable.addEventListener("dragenter", onDragEnter);
    draggable.addEventListener("dragleave", onDragLeave);
}

// Removes colors move tool listeners
function removeMoveColorsListeners() {
    const colors = document.querySelectorAll<HTMLElement>("[id*='colorRectangle']");

    colors.forEach(color => {
        color.removeEventListener("dragstart", onDragStart);
        color.removeEventListener("dragend", onDragEnd);
        color.removeEventListener("dragover", onDragOver);
        color.removeEventListener("dragenter", onDragEnter);
        color.removeEventListener("dragleave", onDragLeave);
    });
};

function onDragStart(event: Event) {
    draggingElement = event.currentTarget as HTMLElement;
    draggingElement.classList.add("dragging");
}

function onDragEnd(event: Event) {
    if (draggingElement && replacingElement && draggingElement.dataset.color !== replacingElement.dataset.color)
        swapColors(draggingElement, replacingElement);

    draggingElement?.classList.remove("dragging");
    replacingElement?.classList.remove("replacing");
}

function onDragOver(event: Event) {
    event.preventDefault();
    replacingElement = event.currentTarget as HTMLElement;
    replacingElement.classList.add("replacing");
}

function onDragEnter() {
    mouseOverColor = true;
}

function onDragLeave(event: Event) {
    const draggable = event.currentTarget as HTMLElement;
    mouseOverColor = false;
    draggable.classList.remove("replacing");
}

// Swaps saved colors, saves to local storage and renders colors grid
function swapColors(drag: HTMLElement, replace: HTMLElement) {
    const savedColors = JSON.parse(localStorage.getItem("colorpal-saved-colors-array") || "[]");
    const [dragIndex, replaceIndex] = [savedColors.indexOf(drag.dataset.color), savedColors.indexOf(replace.dataset.color)];

    if (dragIndex === -1 || replaceIndex === -1)
        return;

    [savedColors[dragIndex], savedColors[replaceIndex]] = [replace.dataset.color, drag.dataset.color];

    localStorage.setItem("colorpal-saved-colors-array", JSON.stringify(savedColors));
    colorsGridComponent.invokeMethodAsync("RenderSavedColorsAsync", null);
}
