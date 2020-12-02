var fancySlider = /** @class */ (function () {
    function fancySlider() {
        this.init = function (slider, options) {
            console.log(slider);
            console.log(options);
        };
    }
    fancySlider.getInstance = function () {
        if (!this._instance)
            this._instance = new fancySlider();
        return this._instance;
    };
    fancySlider.initialize = function (sliderSelector, options) {
        var sliders = document.getElementsByClassName(sliderSelector);
        Array(sliders).forEach(function (slider) {
            fancySlider.getInstance().init(slider, options);
        });
    };
    return fancySlider;
}());
window.initialize_slider = function (isInitialize) {
    console.log('initialize_slider');
    fancySlider.initialize("example-slider", { autoSlidingDelay: 4000, isInitialize: isInitialize });
};
//# sourceMappingURL=fancy_slider2.js.map