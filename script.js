const eyeDropperButton = document.querySelector("#eye-dropper-button");
const colorPalette = document.querySelector("#color-palette");
const settingsButton = document.querySelector("#settings-button");
const copyRGBButton = document.querySelector("#copy-rgb-button");
const copyHexButton = document.querySelector("#copy-hex-button");
const copyHslButton = document.querySelector("#copy-hsl-button");
const copyHsvButton = document.querySelector("#copy-hsv-button");
const colorsTools = document.querySelector(".colors-tools");
const settingsTools = document.querySelector(".settings-tools");
const colorsButton = document.querySelector("#colors-button");
const lightDarkButton = document.querySelector("#light-dark-mode");
const allColors = document.querySelector(".all-colors");
const savedColorsCount = document.querySelector(".saved-colors-count");
const deleteOnClick = document.querySelector("#delete-on-click");
const deleteAll = document.querySelector("#delete-all");
const lightModeIcon = document.querySelector("#light-mode-icon");
const root = document.querySelector(":root");
const settingsPanel = document.querySelector(".settings-panel");
const codesMessages = document.querySelector(".codes-messages");
const saveSelectedColor = document.querySelector(".save-selected-color");
const savedColors = document.querySelector(".saved-colors");
const selectedColor = document.querySelector(".selected-color .rect");
const saveColorButton = document.querySelector(".save-color-button");
const displayMessages = document.querySelector(".display-messages");
const displayMessageText = document.querySelector("#text");
const displayMessageColorCode = document.querySelector("#color-code");
const selectedColorRGB = document.querySelector("#rgb");
const selectedColorHex = document.querySelector("#hex");
const selectedColorHSL = document.querySelector("#hsl");
const selectedColorHSV = document.querySelector("#hsv");
const autoSaveEyeDropper = document.querySelector("#save-eye-dropper");
const autoCopyColorCode = document.querySelector("#copy-color-code");
const colorCodeFormat = document.querySelector("#code-format");
const colorsPerLine = document.querySelector("#colors-per-line");
const displayMessagesOption = document.querySelector(
  "#display-messages-option"
);
const savedColorsArray = JSON.parse(
  localStorage.getItem("savedColorsArray") ?? "[]"
);
var messageTimeout,
  deleteColorOnClick = false;

setOptions();
showPage("colors");

function setOptions() {
  setLightDarkMode(localStorage.getItem("lightDarkMode"));

  localStorage.getItem("currSelectedColor") === null
    ? applyCurrentSelectedColor("#000000")
    : applyCurrentSelectedColor(localStorage.getItem("currSelectedColor"));

  localStorage.getItem("autoSaveEyeDropper") === null &&
    localStorage.setItem("autoSaveEyeDropper", "true");

  localStorage.getItem("autoCopyColorCode") === null &&
    localStorage.setItem("autoCopyColorCode", "true");

  localStorage.getItem("colorCodeFormat") === null &&
    localStorage.setItem("colorCodeFormat", "HEX");
  colorCodeFormat.value = localStorage.getItem("colorCodeFormat");

  localStorage.getItem("colorsPerLine") === null &&
    localStorage.setItem("colorsPerLine", "8");
  colorsPerLine.value = localStorage.getItem("colorsPerLine");
  setColorsPerLine(parseInt(localStorage.getItem("colorsPerLine")));

  localStorage.getItem("displayMessagesOption") === null &&
    localStorage.setItem("displayMessagesOption", "true");

  localStorage.getItem("lightDarkMode") === null &&
    localStorage.setItem("lightDarkMode", "light");

  localStorage.getItem("autoSaveEyeDropper") === "true"
    ? (autoSaveEyeDropper.checked = true)
    : (autoSaveEyeDropper.checked = false);
  localStorage.getItem("autoCopyColorCode") === "true"
    ? (autoCopyColorCode.checked = true)
    : (autoCopyColorCode.checked = false);
  localStorage.getItem("displayMessagesOption") === "true"
    ? (displayMessagesOption.checked = true)
    : (displayMessagesOption.checked = false);
}

function setLightDarkMode(mode) {
  if (mode === "dark") {
    localStorage.setItem("lightDarkMode", "dark");
    root.style.setProperty("--first-color", "#24282a");
    root.style.setProperty("--second-color", "#2b353e");
    root.style.setProperty("--text-color", "#fafcff");
    lightModeIcon.setAttribute("class", "bx bxs-sun");
  } else {
    // light
    localStorage.setItem("lightDarkMode", "light");
    root.style.setProperty("--first-color", "#fafcff");
    root.style.setProperty("--second-color", "#e7e7f4");
    root.style.setProperty("--text-color", "#24282a");
    lightModeIcon.setAttribute("class", "bx bxs-moon");
  }
}

function setColorsPerLine(colorsPerLine) {
  switch (colorsPerLine) {
    case 6:
      allColors.style.setProperty("grid-template-columns", "repeat(6, 1fr)");
      root.style.setProperty("--rect-height", "71.1px");
      root.style.setProperty("--rect-width", "71.1px");
      root.style.setProperty("--rect-margin", "6.5px");
      break;
    case 8:
      allColors.style.setProperty("grid-template-columns", "repeat(8, 1fr)");
      root.style.setProperty("--rect-height", "53.1px");
      root.style.setProperty("--rect-width", "53.1px");
      root.style.setProperty("--rect-margin", "5px");
      break;
    case 10:
      allColors.style.setProperty("grid-template-columns", "repeat(10, 1fr)");
      root.style.setProperty("--rect-height", "42.1px");
      root.style.setProperty("--rect-width", "42.1px");
      root.style.setProperty("--rect-margin", "4.2px");
      break;
    case 12:
      allColors.style.setProperty("grid-template-columns", "repeat(12, 1fr)");
      root.style.setProperty("--rect-height", "35.1px");
      root.style.setProperty("--rect-width", "35.1px");
      root.style.setProperty("--rect-margin", "3.5px");
      break;
  }
}

function showPage(page) {
  if (page === "colors") {
    !savedColorsArray.length
      ? savedColors.classList.add("hide")
      : savedColors.classList.remove("hide");

    saveSelectedColor.classList.remove("hide");
    codesMessages.classList.remove("hide");
    colorsTools.classList.remove("hide");
    settingsTools.classList.add("hide");
    settingsPanel.classList.add("hide");
    displaySavedColorsCount(savedColorsArray.length);
    showColors();
  } else {
    // settings
    savedColors.classList.add("hide");
    saveSelectedColor.classList.add("hide");
    codesMessages.classList.add("hide");
    colorsTools.classList.add("hide");
    settingsTools.classList.remove("hide");
    settingsPanel.classList.remove("hide");
    deleteColorOnClick && setDeleteOnClick(false, false);
  }
}

function applyCurrentSelectedColor(color) {
  localStorage.setItem("currSelectedColor", color);
  savedColorsArray.includes(color)
    ? saveColorButton.classList.add("hide")
    : saveColorButton.classList.remove("hide");

  colorPalette.value = localStorage.getItem("currSelectedColor");
  selectedColor.style.background = color;
  selectedColorHex.textContent = color;
  root.style.setProperty(
    "--selected-color",
    localStorage.getItem("currSelectedColor")
  );

  selectedColorRGB.textContent = hexToRgb(color, true);
  let rgbColor = hexToRgb(color, false);
  selectedColorHSL.textContent = rgbToHsl(rgbColor, true);
  selectedColorHSV.textContent = rgbToHsv(rgbColor, true);
}

function showColors() {
  if (!savedColorsArray.length) {
    savedColors.classList.add("hide");
    return;
  }
  savedColors.classList.remove("hide");

  // add li for each color
  allColors.innerHTML = savedColorsArray
    .map(
      (color) => `
    <li class="color">
        <span class="${
          deleteColorOnClick ? "rect-delete" : "rect"
        }" data-color="${color}" style="background: ${color};">${
        deleteColorOnClick ? "<i class='bx bx-trash-alt' ></i>" : ""
      }</span>
    </li>`
    )
    .join("");

  // add listeners
  document.querySelectorAll(".color").forEach((li) => {
    li.addEventListener("click", (element) =>
      savedColorClicked(element.currentTarget.lastElementChild)
    );
  });
}

function displayMessageAndColor(text, color, colorFormat) {
  if (localStorage.getItem("displayMessagesOption") === "false") return;

  displayMessages.classList.remove("hide");
  displayMessageText.textContent = text;
  displayMessageColorCode.textContent = displayColorString(color, colorFormat);

  // hide message after 2 seconds
  clearTimeout(messageTimeout);
  messageTimeout = setTimeout(
    () => displayMessages.classList.add("hide"),
    2000
  );
}

function displayColorString(color, format) {
  if (color === null || format === null) return;

  let rgbColor = hexToRgb(color, false);
  switch (format) {
    case "RGB":
      return hexToRgb(color, true);
    case "HEX":
      return color;
    case "HSL":
      return rgbToHsl(rgbColor, true);
    case "HSV":
      return rgbToHsv(rgbColor, true);
  }
}

function savedColorClicked(colorClicked) {
  if (deleteColorOnClick) {
    applyCurrentSelectedColor(colorClicked.dataset.color);
    deleteColor(colorClicked.dataset.color);
    return;
  }

  localStorage.getItem("autoCopyColorCode") === "true" &&
    copyToClipboard(
      colorClicked.dataset.color,
      localStorage.getItem("colorCodeFormat")
    );

  applyCurrentSelectedColor(colorClicked.dataset.color);

  localStorage.getItem("autoCopyColorCode") === "true"
    ? (text = "Copied")
    : (text = "Selected");

  displayMessageAndColor(
    text,
    localStorage.getItem("currSelectedColor"),
    localStorage.getItem("colorCodeFormat")
  );
}

function saveColor(color) {
  if (savedColorsArray.includes(color)) {
    displayMessageAndColor(
      "Already Saved",
      color,
      localStorage.getItem("colorCodeFormat")
    );
    return;
  }

  savedColorsArray.push(color);
  localStorage.setItem("savedColorsArray", JSON.stringify(savedColorsArray));
  showColors();
  saveColorButton.classList.add("hide");

  localStorage.getItem("autoCopyColorCode") === "true"
    ? (text = "Saved and Copied")
    : (text = "Saved");

  displayMessageAndColor(text, color, localStorage.getItem("colorCodeFormat"));

  localStorage.getItem("autoCopyColorCode") === "true" &&
    copyToClipboard(color, localStorage.getItem("colorCodeFormat"));

  displaySavedColorsCount(savedColorsArray.length);
}

function saveColorButtonClicked() {
  saveColor(localStorage.getItem("currSelectedColor"));
  saveColorButton.classList.add("hide");
}

function copyToClipboard(color, format) {
  let rgbColor = hexToRgb(color, false);
  switch (format) {
    case "RGB":
      navigator.clipboard.writeText(hexToRgb(color, true));
      break;
    case "HEX":
      navigator.clipboard.writeText(color);
      break;
    case "HSL":
      navigator.clipboard.writeText(rgbToHsl(rgbColor, true));
      break;
    case "HSV":
      navigator.clipboard.writeText(rgbToHsv(rgbColor, true));
      break;
  }
}

function activateEyeDropper() {
  document.body.style.display = "none";
  deleteOnClick && setDeleteOnClick(false, true);
  setTimeout(async () => {
    try {
      const eyeDropper = new EyeDropper();
      const { sRGBHex } = await eyeDropper.open(); // hex color
      applyCurrentSelectedColor(sRGBHex);

      if (localStorage.getItem("autoSaveEyeDropper") === "true") {
        saveColor(sRGBHex);
        localStorage.getItem("autoCopyColorCode") === "true" &&
          copyToClipboard(sRGBHex, localStorage.getItem("colorCodeFormat"));
      } else
        displayMessageAndColor(
          "Selected",
          sRGBHex,
          localStorage.getItem("colorCodeFormat")
        );
    } catch {
      displayMessageAndColor("Closed Eye Dropper", null, null);
    }
    document.body.style.display = "block";
  }, 10);
}

function deleteColor(color) {
  let colorIndex = savedColorsArray.findIndex((clr) => {
    return clr === color;
  });

  savedColorsArray.splice(colorIndex, 1);
  localStorage.setItem("savedColorsArray", JSON.stringify(savedColorsArray));

  displaySavedColorsCount(savedColorsArray.length);
  showColors();
  saveColorButton.classList.remove("hide");

  displayMessageAndColor(
    "Deleted",
    color,
    localStorage.getItem("colorCodeFormat")
  );

  if (!savedColorsArray.length) {
    applyCurrentSelectedColor("#000000");
    deleteOnClick && setDeleteOnClick(false, true);
    showColors();
  }
}

function deleteAllColors() {
  if (confirm("Delete All Your Colors?")) {
    savedColorsArray.length = 0;
    localStorage.setItem("savedColorsArray", "[]");
    displaySavedColorsCount(0);
    displayMessageAndColor("Deleted All", null, null);
    applyCurrentSelectedColor("#000000");
    deleteOnClick && setDeleteOnClick(false, true);
    showColors();
  }
}

function setDeleteOnClick(setDelete, reRenderColors) {
  deleteColorOnClick = setDelete;
  deleteColorOnClick
    ? deleteOnClick.setAttribute("class", "bx bx-check")
    : deleteOnClick.setAttribute("class", "bx bx-trash");

  reRenderColors && showColors();
}

function displaySavedColorsCount(count) {
  count === 1
    ? (savedColorsCount.textContent = `1 Color`)
    : (savedColorsCount.textContent = `${count} Colors`);
}

function hexToRgb(hex, returnString) {
  let result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  let r = parseInt(result[1], 16);
  let g = parseInt(result[2], 16);
  let b = parseInt(result[3], 16);

  if (returnString) return `rgb(${r}, ${g}, ${b})`;
  else return { r, g, b };
}

function rgbToHsl(rbg, returnString) {
  (r = rbg.r / 255), (g = rbg.g / 255), (b = rbg.b / 255);
  let max = Math.max(r, g, b),
    min = Math.min(r, g, b);
  let h,
    s,
    l = (max + min) / 2;

  if (max === min) {
    h = s = 0;
  } else {
    let d = max - min;
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
    switch (max) {
      case r:
        h = (g - b) / d + (g < b ? 6 : 0);
        break;
      case g:
        h = (b - r) / d + 2;
        break;
      case b:
        h = (r - g) / d + 4;
        break;
    }
    h /= 6;
  }

  h = Math.round(h * 360);
  s = Math.round(s * 100);
  l = Math.round(l * 100);

  if (returnString) return `hsl(${h}, ${s}%, ${l}%)`;
  else return { h, s, l };
}

function rgbToHsv(rbg, returnString) {
  let rabs, gabs, babs, rr, gg, bb, h, s, v, diff, diffc, percentRoundFn;
  rabs = rbg.r / 255;
  gabs = rbg.g / 255;
  babs = rbg.b / 255;
  (v = Math.max(rabs, gabs, babs)), (diff = v - Math.min(rabs, gabs, babs));
  diffc = (c) => (v - c) / 6 / diff + 1 / 2;
  percentRoundFn = (num) => Math.round(num * 100) / 100;
  if (diff === 0) {
    h = s = 0;
  } else {
    s = diff / v;
    rr = diffc(rabs);
    gg = diffc(gabs);
    bb = diffc(babs);

    if (rabs === v) {
      h = bb - gg;
    } else if (gabs === v) {
      h = 1 / 3 + rr - bb;
    } else if (babs === v) {
      h = 2 / 3 + gg - rr;
    }
    if (h < 0) {
      h += 1;
    } else if (h > 1) {
      h -= 1;
    }
  }

  h = Math.round(h * 360);
  s = Math.round(s * 100);
  v = Math.round(v * 100);

  if (returnString) return `hsv(${h}, ${s}%, ${v}%)`;
  else return { h, s, v };
}

eyeDropperButton.addEventListener("click", activateEyeDropper);
settingsButton.addEventListener("click", function () {
  showPage("settings");
});
colorsButton.addEventListener("click", function () {
  showPage("colors");
});
lightDarkButton.addEventListener("click", function () {
  localStorage.getItem("lightDarkMode") === "light"
    ? setLightDarkMode("dark")
    : setLightDarkMode("light");
});
copyRGBButton.addEventListener("click", function () {
  copyToClipboard(localStorage.getItem("currSelectedColor"), "RGB");
  displayMessageAndColor(
    "Copied",
    localStorage.getItem("currSelectedColor"),
    "RGB"
  );
});
copyHexButton.addEventListener("click", function () {
  copyToClipboard(localStorage.getItem("currSelectedColor"), "HEX");
  displayMessageAndColor(
    "Copied",
    localStorage.getItem("currSelectedColor"),
    "HEX"
  );
});
copyHslButton.addEventListener("click", function () {
  copyToClipboard(localStorage.getItem("currSelectedColor"), "HSL");
  displayMessageAndColor(
    "Copied",
    localStorage.getItem("currSelectedColor"),
    "HSL"
  );
});
copyHsvButton.addEventListener("click", function () {
  copyToClipboard(localStorage.getItem("currSelectedColor"), "HSV");
  displayMessageAndColor(
    "Copied",
    localStorage.getItem("currSelectedColor"),
    "HSV"
  );
});
saveColorButton.addEventListener("click", saveColorButtonClicked);
selectedColor.addEventListener("click", function () {
  colorPalette.click();
  deleteOnClick && setDeleteOnClick(false, true);
});
deleteOnClick.addEventListener("click", function () {
  setDeleteOnClick(!deleteColorOnClick, true);
});
deleteAll.addEventListener("click", deleteAllColors);

colorPalette.addEventListener("input", function () {
  applyCurrentSelectedColor(colorPalette.value);
  displayMessageAndColor(
    "Selected",
    colorPalette.value,
    localStorage.getItem("colorCodeFormat")
  );
});

colorPalette.addEventListener("click", function () {
  deleteOnClick && setDeleteOnClick(false, true);
});

autoSaveEyeDropper.addEventListener("change", function () {
  this.checked
    ? localStorage.setItem("autoSaveEyeDropper", "true")
    : localStorage.setItem("autoSaveEyeDropper", "false");
});

autoCopyColorCode.addEventListener("change", function () {
  this.checked
    ? localStorage.setItem("autoCopyColorCode", "true")
    : localStorage.setItem("autoCopyColorCode", "false");
});

colorCodeFormat.addEventListener("change", function () {
  localStorage.setItem("colorCodeFormat", colorCodeFormat.value);
});

colorsPerLine.addEventListener("change", function () {
  localStorage.setItem("colorsPerLine", colorsPerLine.value);
  setColorsPerLine(parseInt(colorsPerLine.value));
});

displayMessagesOption.addEventListener("change", function () {
  this.checked
    ? localStorage.setItem("displayMessagesOption", "true")
    : localStorage.setItem("displayMessagesOption", "false");
});
