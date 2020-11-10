(function () {

    var $$ = function (selector, context) {
        var context = context || document;
        var elements = context.querySelectorAll(selector);
        return [].slice.call(elements);
    };
    var numOfSlides = 0;
    var autoSlidingTO;

    function _fncSliderInit($slider, options) {
        var prefix = ".fnc-";

        var $slider = $slider;
        var $slidesCont = $slider.querySelector(prefix + "slider__slides");
        var $slides = $$(prefix + "slide", $slider);
        var $controls = $$(prefix + "nav__control", $slider);
        var $controlsBgs = $$(prefix + "nav__bg", $slider);
        var $progressAS = $$(prefix + "nav__control-progress", $slider);

        numOfSlides = $slides.length;
        var curSlide = 1;
        var sliding = false;
        var slidingAT = +parseFloat(getComputedStyle($slidesCont)["transition-duration"]) * 1000;
        var slidingDelay = +parseFloat(getComputedStyle($slidesCont)["transition-delay"]) * 1000;

        var autoSlidingActive = false;
        var autoSlidingDelay = 5000; // default autosliding delay value
        var autoSlidingBlocked = false;

        var $activeSlide;
        var $activeControlsBg;
        var $prevControl;

        function setIDs() {
            $slides.forEach(function ($slide, index) {
                const key = "fnc-slide-" + (index + 1);
                if (!$slide.classList.contains(key)) {
                    $slide.classList.add(key);
                }
            });

            $controls.forEach(function ($control, index) {
                $control.setAttribute("data-slide", index + 1);
                const key = "fnc-nav__control-" + (index + 1);
                if (!$control.classList.contains(key)) {
                    $control.classList.add(key);
                }
            });

            $controlsBgs.forEach(function ($bg, index) {
                const key = "fnc-nav__bg-" + (index + 1);
                if (!$bg.classList.contains(key)) {
                    $bg.classList.add(key);
                }
            });
        };

        setIDs();

        // スライドアニメーション後の待機時間
        function afterSlidingHandler() {
            $slider.querySelector(".m--previous-slide").classList.remove("m--active-slide", "m--previous-slide");
            $slider.querySelector(".m--previous-nav-bg").classList.remove("m--active-nav-bg", "m--previous-nav-bg");

            $activeSlide.classList.remove("m--before-sliding");
            $activeControlsBg.classList.remove("m--nav-bg-before");
            $prevControl.classList.remove("m--prev-control");
            $prevControl.classList.add("m--reset-progress");
            var triggerLayout = $prevControl.offsetTop;
            $prevControl.classList.remove("m--reset-progress");

            sliding = false;
            var layoutTrigger = $slider.offsetTop;

            if (autoSlidingActive && !autoSlidingBlocked) {
                setAutoslidingTO();
            }
        };

        // スライドアニメーションの表示
        // 
        function performSliding(slideID) {
            if (sliding) return;
            sliding = true;
            window.clearTimeout(autoSlidingTO);
            curSlide = slideID;

            $prevControl = $slider.querySelector(".m--active-control");
            $prevControl.classList.remove("m--active-control");
            $prevControl.classList.add("m--prev-control");
            $slider.querySelector(prefix + "nav__control-" + slideID).classList.add("m--active-control");

            $activeSlide = $slider.querySelector(prefix + "slide-" + slideID);
            $activeControlsBg = $slider.querySelector(prefix + "nav__bg-" + slideID);

            $slider.querySelector(".m--active-slide").classList.add("m--previous-slide");
            $slider.querySelector(".m--active-nav-bg").classList.add("m--previous-nav-bg");

            $activeSlide.classList.add("m--before-sliding");
            $activeControlsBg.classList.add("m--nav-bg-before");

            var layoutTrigger = $activeSlide.offsetTop; // 謎の処理だが、これがないとアニメーションしない

            $activeSlide.classList.add("m--active-slide");
            $activeControlsBg.classList.add("m--active-nav-bg");

            // アニメーションが完了したら
            setTimeout(afterSlidingHandler, slidingAT + slidingDelay);
        };

        function controlClickHandler() {
            if (sliding) return;
            if (this.classList.contains("m--active-control")) return;
            if (options.blockASafterClick) {
                autoSlidingBlocked = true;
                $slider.classList.add("m--autosliding-blocked");
            }

            var slideID = +this.getAttribute("data-slide");

            performSliding(slideID);
        };

        $controls.forEach(function ($control, index) {
            const key = "fnc-listener-add-" + (index + 1);
            if (!$control.classList.contains(key)) {
                $control.classList.add(key);
                $control.addEventListener("click", controlClickHandler);
            }
        });

        function setAutoslidingTO() {
            window.clearTimeout(autoSlidingTO);
            var delay = +options.autoSlidingDelay || autoSlidingDelay;
            curSlide++;
            if (curSlide > numOfSlides) curSlide = 1;

            autoSlidingTO = setTimeout(function () {
                performSliding(curSlide);
            }, delay);
        };

        if (options.isInitialize) {
            if (options.autoSliding || +options.autoSlidingDelay > 0) {
                if (options.autoSliding === false) return;

                autoSlidingActive = true;

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
        } else {
            var delay = +options.autoSlidingDelay || autoSlidingDelay;
            delay += slidingDelay + slidingAT;

            $progressAS.forEach(function ($progress) {
                $progress.style.transition = "transform " + (delay / 1000) + "s";
            });
        }

    };

    var fncSlider = function (sliderSelector, options) {
        var $sliders = $$(sliderSelector);

        $sliders.forEach(function ($slider) {
            _fncSliderInit($slider, options);
        });
    };

    window.fncSlider = fncSlider;
}());

/* not part of the slider scripts */

/* Slider initialization
options:
autoSliding - boolean
autoSlidingDelay - delay in ms. If audoSliding is on and no value provided, default value is 5000
blockASafterClick - boolean. If user clicked any sliding control, autosliding won't start again
*/
window.initialize_slider = (isInitialize) => {
    fncSlider(".example-slider", { autoSlidingDelay: 4000, isInitialize: isInitialize });
}
