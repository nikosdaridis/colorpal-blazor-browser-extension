interface ColorRGB {
    r: number;
    g: number;
    b: number;
}

interface ColorHSL {
    h: number;
    s: number;
    l: number;
}

interface ColorHSV {
    h: number;
    s: number;
    v: number;
}

interface ColorName {
    name: string;
    rgb: ColorRGB;
}

function toHex(num: number): string {
    return Math.round(num).toString(16).padStart(2, "0");
}

function hexToRgb(hex: string): ColorRGB;
function hexToRgb(hex: string, returnType: "string"): string;
function hexToRgb(hex: string, returnType?: "string" | "ColorRGB"): string | ColorRGB {
    const [, r, g, b] = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i
        .exec(hex)
        ?.map((val) => parseInt(val, 16)) || [0, 0, 0];

    return returnType === "string" ? `rgb(${r}, ${g}, ${b})` : { r, g, b };
}

function hexToHsl(hex: string): ColorHSL;
function hexToHsl(hex: string, returnType: "string"): string;
function hexToHsl(hex: string, returnType?: "string" | "ColorHSL"): string | ColorHSL {
    const hsl = rgbToHsl(hexToRgb(hex));

    return returnType === "string" ? `hsl(${hsl.h}, ${hsl.s}%, ${hsl.l}%)` : hsl;
}

function hexToHsv(hex: string): ColorHSV;
function hexToHsv(hex: string, returnType: "string"): string;
function hexToHsv(hex: string, returnType?: "string" | "ColorHSV"): string | ColorHSV {
    const hsv = rgbToHsv(hexToRgb(hex));

    return returnType === "string" ? `hsv(${hsv.h}, ${hsv.s}%, ${hsv.v}%)` : hsv;
}

function rgbToHex(rgb: ColorRGB): string {
    return `#${toHex(rgb.r)}${toHex(rgb.g)}${toHex(rgb.b)}`.toLowerCase();
}

function rgbToHsl(rgb: ColorRGB): ColorHSL;
function rgbToHsl(rgb: ColorRGB, returnType: "string"): string;
function rgbToHsl(rgb: ColorRGB, returnType?: "ColorHSL" | "string"): string | ColorHSL {
    const normalizedRgb = normalizeRgb(rgb);
    const max = Math.max(normalizedRgb.r, normalizedRgb.g, normalizedRgb.b);
    const min = Math.min(normalizedRgb.r, normalizedRgb.g, normalizedRgb.b);
    let h = 0,
        s = 0,
        l = (max + min) / 2;

    if (max === min) {
        h = s = 0;
    } else {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

        switch (max) {
            case normalizedRgb.r:
                h = (normalizedRgb.g - normalizedRgb.b) / d + (normalizedRgb.g < normalizedRgb.b ? 6 : 0);
                break;
            case normalizedRgb.g:
                h = (normalizedRgb.b - normalizedRgb.r) / d + 2;
                break;
            case normalizedRgb.b:
                h = (normalizedRgb.r - normalizedRgb.g) / d + 4;
                break;
        }
        h /= 6;
    }

    h = Math.round(h * 360);
    s = Math.round(s * 100);
    l = Math.round(l * 100);

    return returnType === "string" ? `hsl(${h}, ${s}%, ${l}%)` : { h, s, l };
}

function rgbToHsv(rgb: ColorRGB): ColorHSV;
function rgbToHsv(rgb: ColorRGB, returnType: "string"): string;
function rgbToHsv(rgb: ColorRGB, returnType?: "ColorHSV" | "string"): string | ColorHSV {
    const normalizedRgb = normalizeRgb(rgb);
    let v = Math.max(normalizedRgb.r, normalizedRgb.g, normalizedRgb.b);
    const diff = v - Math.min(normalizedRgb.r, normalizedRgb.g, normalizedRgb.b);
    let h = 0, s = 0;

    if (v === 0) {
        h = s = 0;
    } else {
        s = diff / v;
        if (diff !== 0) {
            switch (v) {
                case normalizedRgb.r:
                    h = (normalizedRgb.g - normalizedRgb.b) / diff;
                    break;
                case normalizedRgb.g:
                    h = 2 + (normalizedRgb.b - normalizedRgb.r) / diff;
                    break;
                case normalizedRgb.b:
                    h = 4 + (normalizedRgb.r - normalizedRgb.g) / diff;
                    break;
            }
        }
        h = (h * 60 + 360) % 360;
    }

    v = Math.round(v * 100);
    s = Math.round(s * 100);

    return returnType === "string" ? `hsv(${Math.round(h)}, ${s}%, ${v}%)` : { h: Math.round(h), s, v };
}

function hslToHex(h: number, s: number, l: number): string {
    h /= 360;
    s /= 100;
    l /= 100;

    let r, g, b;

    if (s === 0) {
        r = g = b = l;
    } else {
        function hueToRgb(p: number, q: number, t: number): number {
            if (t < 0) {
                t += 1;
            }
            if (t > 1) {
                t -= 1;
            }
            return t < 1 / 6
                ? p + (q - p) * 6 * t
                : t < 1 / 2
                    ? q
                    : t < 2 / 3
                        ? p + (q - p) * (2 / 3 - t) * 6
                        : p;
        }
        const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        const p = 2 * l - q;
        r = hueToRgb(p, q, h + 1 / 3);
        g = hueToRgb(p, q, h);
        b = hueToRgb(p, q, h - 1 / 3);
    }

    return `#${toHex(r * 255)}${toHex(g * 255)}${toHex(b * 255)}`;
}

function hslToHexRange(h: number, s: number): string[] {
    return Array.from({ length: 99 }, (_, index) => hslToHex(h, s, index + 1));
}

function normalizeRgb(rgb: ColorRGB) {
    return { r: rgb.r / 255, g: rgb.g / 255, b: rgb.b / 255 };
}

function hexToFilter(hex: string): { filter: string, loss: number } {
    const colorFilter = new ColorFilter(hexToRgb(hex));

    return new FilterSolver(colorFilter).solve();
}

class ColorFilter {
    colorRGB: ColorRGB;

    constructor(rgb: ColorRGB) {
        this.colorRGB = rgb;
    }

    set(rgb: ColorRGB) {
        this.colorRGB = rgb;
    }

    hueRotate(angle: number = 0): void {
        const radians = angle / 180 * Math.PI;
        const sin = Math.sin(radians);
        const cos = Math.cos(radians);

        this.multiply([
            0.213 + cos * 0.787 - sin * 0.213,
            0.715 - cos * 0.715 - sin * 0.715,
            0.072 - cos * 0.072 + sin * 0.928,
            0.213 - cos * 0.213 + sin * 0.143,
            0.715 + cos * 0.285 + sin * 0.140,
            0.072 - cos * 0.072 - sin * 0.283,
            0.213 - cos * 0.213 - sin * 0.787,
            0.715 - cos * 0.715 + sin * 0.715,
            0.072 + cos * 0.928 + sin * 0.072
        ]);
    }

    sepia(value: number = 1): void {
        this.multiply([
            0.393 + 0.607 * (1 - value),
            0.769 - 0.769 * (1 - value),
            0.189 - 0.189 * (1 - value),
            0.349 - 0.349 * (1 - value),
            0.686 + 0.314 * (1 - value),
            0.168 - 0.168 * (1 - value),
            0.272 - 0.272 * (1 - value),
            0.534 - 0.534 * (1 - value),
            0.131 + 0.869 * (1 - value)
        ]);
    }

    saturate(value: number = 1): void {
        this.multiply([
            0.213 + 0.787 * value,
            0.715 - 0.715 * value,
            0.072 - 0.072 * value,
            0.213 - 0.213 * value,
            0.715 + 0.285 * value,
            0.072 - 0.072 * value,
            0.213 - 0.213 * value,
            0.715 - 0.715 * value,
            0.072 + 0.928 * value
        ]);
    }

    multiply(matrix: number[]): void {
        const newR = this.clamp(this.colorRGB.r * matrix[0] + this.colorRGB.g * matrix[1] + this.colorRGB.b * matrix[2]);
        const newG = this.clamp(this.colorRGB.r * matrix[3] + this.colorRGB.g * matrix[4] + this.colorRGB.b * matrix[5]);
        const newB = this.clamp(this.colorRGB.r * matrix[6] + this.colorRGB.g * matrix[7] + this.colorRGB.b * matrix[8]);
        this.colorRGB.r = newR;
        this.colorRGB.g = newG;
        this.colorRGB.b = newB;
    }

    brightness(value: number = 1): void {
        this.linear(value);
    }

    contrast(value: number = 1): void {
        this.linear(value, -(0.5 * value) + 0.5);
    }

    linear(slope: number = 1, intercept: number = 0): void {
        this.colorRGB.r = this.clamp(this.colorRGB.r * slope + intercept * 255);
        this.colorRGB.g = this.clamp(this.colorRGB.g * slope + intercept * 255);
        this.colorRGB.b = this.clamp(this.colorRGB.b * slope + intercept * 255);
    }

    invert(value: number = 1): void {
        this.colorRGB.r = this.clamp((value + this.colorRGB.r / 255 * (1 - 2 * value)) * 255);
        this.colorRGB.g = this.clamp((value + this.colorRGB.g / 255 * (1 - 2 * value)) * 255);
        this.colorRGB.b = this.clamp((value + this.colorRGB.b / 255 * (1 - 2 * value)) * 255);
    }

    clamp(value: number): number {
        return Math.max(0, Math.min(255, value));
    }
}

class FilterSolver {
    colorFilter: ColorFilter;
    colorHSL: ColorHSL;
    reusedColorFilter: ColorFilter;

    constructor(colorFilter: ColorFilter) {
        this.colorFilter = colorFilter;
        this.colorHSL = rgbToHsl(colorFilter.colorRGB);
        this.reusedColorFilter = new ColorFilter({ r: 0, g: 0, b: 0 });
    }

    solve(): { filter: string, loss: number } {
        const wideResult = this.solveWide();
        const narrowResult = this.solveNarrow(wideResult);

        return { filter: this.filterCSS(narrowResult.values), loss: narrowResult.loss };
    }

    solveWide(): { values: number[]; loss: number } {
        const A = 5;
        const c = 15;
        const a = [60, 180, 18000, 600, 1.2, 1.2];
        let best: { values: number[]; loss: number } = { values: [], loss: Infinity };
        let attempt = 0;

        while (best.loss > 0.5 && attempt < 20) {
            const initial = [50, 20, 3750, 50, 100, 100];
            const result = this.spsa(A, a, c, initial, 1000);

            if (result.loss < best.loss)
                best = result;

            attempt++;
        }

        return best;
    }

    solveNarrow(wide: { values: number[]; loss: number }): { values: number[]; loss: number } {
        const A = wide.loss;
        const c = 2;
        const A1 = A + 1;
        const a = [0.25 * A1, 0.25 * A1, A1, 0.25 * A1, 0.2 * A1, 0.2 * A1];
        let best: { values: number[]; loss: number } = { values: wide.values, loss: wide.loss };
        let attempt = 0;

        while (best.loss > 0.05 && attempt < 50) {
            const result = this.spsa(A, a, c, best.values, 500);

            if (result.loss < best.loss)
                best = result;

            attempt++;
        }

        return best;
    }

    spsa(A: number, a: number[], c: number, values: number[], iters: number): { values: number[]; loss: number } {
        const alpha = 1;
        const gamma = 0.16666666666666666;

        let best: number[] = [];
        let bestLoss = Infinity;
        const deltas = new Array(6).fill(0);
        const highArgs = new Array(6).fill(0);
        const lowArgs = new Array(6).fill(0);

        for (let k = 0; k < iters; k++) {
            const ck = c / Math.pow(k + 1, gamma);
            for (let i = 0; i < 6; i++) {
                deltas[i] = Math.random() > 0.5 ? 1 : -1;
                highArgs[i] = values[i] + ck * deltas[i];
                lowArgs[i] = values[i] - ck * deltas[i];
            }

            const lossDiff = this.loss(highArgs) - this.loss(lowArgs);
            for (let i = 0; i < 6; i++) {
                const g = lossDiff / (2 * ck) * deltas[i];
                const ak = a[i] / Math.pow(A + k + 1, alpha);
                values[i] = this.fix(values[i] - ak * g, i);
            }

            const loss = this.loss(values);
            if (loss < bestLoss) {
                best = values.slice();
                bestLoss = loss;
            }
        }

        return { values: best, loss: bestLoss };
    }

    fix(value: number, idx: number): number {
        let max = 100;

        if (idx === 2) {
            max = 7500;
        } else if (idx === 4 || idx === 5) {
            max = 200;
        }


        if (idx === 3) {
            if (value > max)
                value %= max;
            else if (value < 0)
                value = max + value % max;
        } else {
            value = Math.max(0, Math.min(max, value));
        }

        return value;
    }

    loss(filters: number[]): number {
        const color = this.reusedColorFilter;
        color.set({ r: 0, g: 0, b: 0 });

        color.invert(filters[0] / 100);
        color.sepia(filters[1] / 100);
        color.saturate(filters[2] / 100);
        color.hueRotate(filters[3] * 3.6);
        color.brightness(filters[4] / 100);
        color.contrast(filters[5] / 100);

        const colorHSL = rgbToHsl(color.colorRGB);

        return (
            Math.abs(color.colorRGB.r - this.colorFilter.colorRGB.r) +
            Math.abs(color.colorRGB.g - this.colorFilter.colorRGB.g) +
            Math.abs(color.colorRGB.b - this.colorFilter.colorRGB.b) +
            Math.abs(colorHSL.h - this.colorHSL.h) +
            Math.abs(colorHSL.s - this.colorHSL.s) +
            Math.abs(colorHSL.l - this.colorHSL.l)
        );
    }

    filterCSS(filters: number[]): string {
        const fmt = (idx: number, multiplier: number = 1): number => Math.round(filters[idx] * multiplier);
        return `filter: invert(${fmt(0)}%) sepia(${fmt(1)}%) saturate(${fmt(2)}%) hue-rotate(${fmt(3, 3.6)}deg) brightness(${fmt(4)}%) contrast(${fmt(5)}%);`;
    }
}
