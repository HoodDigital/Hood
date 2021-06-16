this.hood = this.hood || {};
this.hood.google = (function (exports) {
    'use strict';

    var center = { lat: 30, lng: -110 };
    function initMaps() {
        new google.maps.Map(document.getElementById("map"), {
            center: center,
            zoom: 8
        });
    }

    exports.initMaps = initMaps;

    Object.defineProperty(exports, '__esModule', { value: true });

    return exports;

}({}));
//# sourceMappingURL=google.js.map
