declare global {
    interface Number {
        formatCurrency(currency: string): string;
        formatKilobytes(): string;
        formatMegabytes(): string;
    }
}


Number.prototype.formatCurrency = function (currency: string) {
    return currency + " " + this.toFixed(2).replace(/./g, function (c: string, i: number, a: string | any[]) {
        return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
    });
}
Number.prototype.formatKilobytes = function() {
    let n = this / 1024;
    return n.toFixed(2).replace(/./g, function (c: string, i: number, a: string | any[]) {
        return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
    });
}
Number.prototype.formatMegabytes = function() {
    let n = this / 1024;    
    n = n / 1024;
    return n.toFixed(2).replace(/./g, function (c: string, i: number, a: string | any[]) {
        return i > 0 && c !== "." && (a.length - i) % 3 === 0 ? "," + c : c;
    });
}

export { }
