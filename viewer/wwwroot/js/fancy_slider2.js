var fancySlider = /** @class */ (function () {
    function fancySlider() {
        var _this = this;
        this.numOfSlides = 0;
        this.nextSlideId = 0;
        this.autoSlidingActive = false;
        this.autoSlidingBlocked = false;
        this.curSlide = 1;
        this.sliding = false;
        this.$$ = function (selector, context) {
            if (context === void 0) { context = null; }
            var context = context || document;
            var elements = document.querySelectorAll(selector);
            return [].slice.call(elements);
        };
        this.setIDs = function (slides, controls, controlsBgs) {
            slides.forEach(function ($slide, index) {
                var key = "fnc-slide-" + (index + 1);
                if (!$slide.classList.contains(key)) {
                    $slide.classList.add(key);
                    if (!this.options.isInitialize && this.nextSlideId == 0) {
                        // 追加されたスライドを次に表示する
                        this.nextSlideId = index + 1;
                    }
                }
            });
            controls.forEach(function ($control, index) {
                $control.setAttribute("data-slide", index + 1);
                var key = "fnc-nav__control-" + (index + 1);
                if (!$control.classList.contains(key)) {
                    $control.classList.add(key);
                }
            });
            controlsBgs.forEach(function ($bg, index) {
                var key = "fnc-nav__bg-" + (index + 1);
                if (!$bg.classList.contains(key)) {
                    $bg.classList.add(key);
                }
            });
        };
        this.init = function (slider, options) {
            _this.options = options;
            var $slider = slider[0];
            var $slidesCont = document.querySelector(fancySlider.prefix + "slider__slides");
            var $slides = _this.$$(fancySlider.prefix + "slide", $slider);
            var $controls = _this.$$(fancySlider.prefix + "nav__control", $slider);
            var $controlsBgs = _this.$$(fancySlider.prefix + "nav__bg", $slider);
            var $progressAS = _this.$$(fancySlider.prefix + "nav__control-progress", $slider);
            _this.numOfSlides = $slides.length;
            var slidingAT = +parseFloat(getComputedStyle($slidesCont)["transition-duration"]) * 1000;
            var slidingDelay = +parseFloat(getComputedStyle($slidesCont)["transition-delay"]) * 1000;
            var autoSlidingDelay = 5000; // default autosliding delay value
            var setIDs = function () {
                $slides.forEach(function ($slide, index) {
                    var key = "fnc-slide-" + (index + 1);
                    if (!$slide.classList.contains(key)) {
                        $slide.classList.add(key);
                        if (!options.isInitialize && this.nextSlideId == 0) {
                            // 追加されたスライドを次に表示する
                            this.nextSlideId = index + 1;
                        }
                    }
                });
                $controls.forEach(function ($control, index) {
                    $control.setAttribute("data-slide", index + 1);
                    var key = "fnc-nav__control-" + (index + 1);
                    if (!$control.classList.contains(key)) {
                        $control.classList.add(key);
                    }
                });
                $controlsBgs.forEach(function ($bg, index) {
                    var key = "fnc-nav__bg-" + (index + 1);
                    if (!$bg.classList.contains(key)) {
                        $bg.classList.add(key);
                    }
                });
            };
            setIDs();
            // スライドアニメーション後の待機時間
            var afterSlidingHandler = function () {
                $slider.querySelector(".m--previous-slide").classList.remove("m--active-slide", "m--previous-slide");
                $slider.querySelector(".m--previous-nav-bg").classList.remove("m--active-nav-bg", "m--previous-nav-bg");
                _this.$activeSlide.classList.remove("m--before-sliding");
                _this.$activeControlsBg.classList.remove("m--nav-bg-before");
                _this.$prevControl.classList.remove("m--prev-control");
                _this.$prevControl.classList.add("m--reset-progress");
                var triggerLayout = _this.$prevControl.offsetTop;
                _this.$prevControl.classList.remove("m--reset-progress");
                _this.sliding = false;
                var layoutTrigger = $slider.offsetTop;
                if (_this.autoSlidingActive && !_this.autoSlidingBlocked) {
                    setAutoslidingTO();
                }
            };
            // スライドアニメーションの表示
            var performSliding = function (slideID) {
                if (_this.sliding)
                    return;
                _this.sliding = true;
                window.clearTimeout(_this.autoSlidingTO);
                // 表示予約があったらそれを表示
                if (_this.nextSlideId > 0) {
                    slideID = _this.nextSlideId;
                    _this.nextSlideId = 0;
                }
                _this.curSlide = slideID;
                _this.$prevControl = $slider.querySelector(".m--active-control");
                _this.$prevControl.classList.remove("m--active-control");
                _this.$prevControl.classList.add("m--prev-control");
                $slider.querySelector(fancySlider.prefix + "nav__control-" + slideID).classList.add("m--active-control");
                _this.$activeSlide = $slider.querySelector(fancySlider.prefix + "slide-" + slideID);
                _this.$activeControlsBg = $slider.querySelector(fancySlider.prefix + "nav__bg-" + slideID);
                $slider.querySelector(".m--active-slide").classList.add("m--previous-slide");
                $slider.querySelector(".m--active-nav-bg").classList.add("m--previous-nav-bg");
                _this.$activeSlide.classList.add("m--before-sliding");
                _this.$activeControlsBg.classList.add("m--nav-bg-before");
                var layoutTrigger = _this.$activeSlide.offsetTop; // 謎の処理だが、これがないとアニメーションしない
                _this.$activeSlide.classList.add("m--active-slide");
                _this.$activeControlsBg.classList.add("m--active-nav-bg");
                // アニメーションが完了したら
                setTimeout(afterSlidingHandler, slidingAT + slidingDelay);
            };
            function controlClickHandler() {
                // 自分自身を表示中だったら処理しない
                if (this.sliding)
                    return;
                if (this.classList.contains("m--active-control"))
                    return;
                // 表示予約があったらキャンセルする
                if (this.nextSlideId > 0) {
                    this.nextSlideId = 0;
                }
                if (options === null || options === void 0 ? void 0 : options.blockASafterClick) {
                    this.autoSlidingBlocked = true;
                    $slider.classList.add("m--autosliding-blocked");
                }
                var slideID = +this.getAttribute("data-slide");
                performSliding(slideID);
            }
            ;
            $controls.forEach(function ($control, index) {
                var key = "fnc-listener-add-" + (index + 1);
                if (!$control.classList.contains(key)) {
                    $control.classList.add(key);
                    $control.addEventListener("click", controlClickHandler);
                }
            });
            var setAutoslidingTO = function () {
                window.clearTimeout(_this.autoSlidingTO);
                var delay = +options.autoSlidingDelay || autoSlidingDelay;
                _this.curSlide++;
                if (_this.curSlide > _this.numOfSlides)
                    _this.curSlide = 1;
                _this.autoSlidingTO = setTimeout(function () {
                    performSliding(_this.curSlide);
                }, delay);
            };
            if (options.isInitialize) {
                if ((options === null || options === void 0 ? void 0 : options.autoSliding) || +options.autoSlidingDelay > 0) {
                    if ((options === null || options === void 0 ? void 0 : options.autoSliding) === false)
                        return;
                    _this.autoSlidingActive = true;
                    setAutoslidingTO();
                    if (!$slider.classList.contains("m--with-autosliding")) {
                        $slider.classList.add("m--with-autosliding");
                    }
                    var triggerLayout = $slider.offsetTop;
                    var delay = +options.autoSlidingDelay || autoSlidingDelay;
                    delay += slidingDelay + slidingAT;
                    $progressAS.forEach(function ($progress) {
                        $progress.style.transition = "transform " + (delay / 1000) + "s";
                    });
                }
                $slider.querySelector(".fnc-nav__control:first-child").classList.add("m--active-control");
            }
            else {
                var delay = +options.autoSlidingDelay || autoSlidingDelay;
                delay += slidingDelay + slidingAT;
                $progressAS.forEach(function ($progress) {
                    $progress.style.transition = "transform " + (delay / 1000) + "s";
                });
            }
        };
    }
    fancySlider.getInstance = function () {
        if (!this._instance)
            this._instance = new fancySlider();
        return this._instance;
    };
    fancySlider.initialize = function (sliderSelector, options) {
        var sliders = fancySlider.getInstance().$$(sliderSelector);
        Array(sliders).forEach(function (slider) {
            fancySlider.getInstance().init(slider, options);
        });
    };
    fancySlider.prefix = '.fnc-';
    return fancySlider;
}());
window.initialize_slider = function (isInitialize) {
    fancySlider.initialize(".example-slider", { autoSlidingDelay: 4000, isInitialize: isInitialize });
};
//# sourceMappingURL=fancy_slider2.js.map