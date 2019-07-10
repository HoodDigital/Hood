String.prototype.contains = function (it) { return this.indexOf(it) !== -1; };
String.prototype.pick = function (min, max) {
    var n, chars = '';

    if (typeof max === 'undefined') {
        n = min;
    } else {
        n = min + Math.floor(Math.random() * (max - min));
    }

    for (var i = 0; i < n; i++) {
        chars += this.charAt(Math.floor(Math.random() * this.length));
    }

    return chars;
};
// Credit to @Christoph: http://stackoverflow.com/a/962890/464744
String.prototype.shuffle = function () {
    var array = this.split('');
    var tmp, current, top = array.length;

    if (top) while (--top) {
        current = Math.floor(Math.random() * (top + 1));
        tmp = array[current];
        array[current] = array[top];
        array[top] = tmp;
    }

    return array.join('');
};
String.prototype.toSeoUrl = function () {
    var output = this.replace(/[^a-zA-Z0-9]/g, ' ').replace(/\s+/g, "-").toLowerCase();
    /* remove first dash */
    if (output.charAt(0) === '-') output = output.substring(1);
    /* remove last dash */
    var last = output.length - 1;
    if (output.charAt(last) === '-') output = output.substring(0, last);
    return output;
};
