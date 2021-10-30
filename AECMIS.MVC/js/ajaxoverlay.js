function ShowProgressBar(el, options) {
    var container = el;
    var defaults = {
        bgColor: '#fff',
        duration: 800,
        opacity: 0.7,
        classOveride: false
    };
    this.options = jQuery.extend(defaults, options);

    // Delete any other loaders
    HideProgressBar(el);
    // Create the overlay 
    var overlay = $('<div></div>').css({
        'background-color': this.options.bgColor,
        'opacity': this.options.opacity,
        'width': '100%',
        'height': container.height(),
        'position': 'absolute',
        'top': '0px',
        'left': '0px',
        'z-index': 99999
    }).addClass('ajax_overlay');
    // add an overiding class name to set new loader style 
    if (this.options.classOveride) {
        overlay.addClass(this.options.classOveride);
    }
    // insert overlay and loader into DOM 
    container.append(
			overlay.append(
				$('<div></div>').addClass('ajax_loader')
			).fadeIn(this.options.duration)
		);
}

function HideProgressBar(el, options) {
    var overlay = el.children(".ajax_overlay");
    if (overlay.length) {
        overlay.fadeOut(this.options.classOveride, function () {
            overlay.remove();
        });
    }
}