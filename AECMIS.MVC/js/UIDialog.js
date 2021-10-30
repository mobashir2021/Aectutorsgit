//ko.bindingHandlers.dialog = {
//    init: function (element, valueAccessor, allBindingsAccessor) {
//        var options = ko.utils.unwrapObservable(valueAccessor()) || {};
//        //do in a setTimeout, so the applyBindings doesn't bind twice from element being copied and moved to bottom
//        setTimeout(function () {
//            $el = $(element);
//            $el.on('hidden.bs.modal', function(e) {
//                allBindingsAccessor().dialogVisible(false);
//            });

//            $(element).modal(options);
//        }, 0);

//        //handle disposal (not strictly necessary in this scenario)
//        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {

//        });
//    },
//    update: function (element, valueAccessor, allBindingsAccessor) {
//        var shouldBeOpen = ko.utils.unwrapObservable(allBindingsAccessor().dialogVisible),
//                        $el = $(element);//,
//        //dialog = $el.data("uiDialog") || $el.data("dialog");
//        //dialog = $el.data("dialog");

//        //don't call open/close before initilization
//        //if (dialog) {
//        $el.modal({show:shouldBeOpen,backdrop: 'static',keyboard: false});
//        //}
//    }
//};

ko.bindingHandlers.executeOnEnter = {
    init: function (element, valueAccessor, allBindings, viewModel) {
        var callback = valueAccessor();
        $(element).keypress(function (event) {
            var keyCode = (event.which ? event.which : event.keyCode);
            if (keyCode === 13) {
                $(element).blur();
                 callback.call(viewModel);
                return false;
            }
            return true;
        });
    }
};



ko.bindingHandlers.dateFormat1 = {
    init: function (element, valueAccessor, allBindings) {
        var $element = $(element);
        ko.utils.registerEventHandler(element, "change", function () {
            var observable = valueAccessor();
            observable($element.val());
        });

    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor(),
            allBindings = allBindingsAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(value);
        var pattern = allBindings.datePattern || 'mmmm d, yyyy';
        if (valueUnwrapped == undefined || valueUnwrapped == null) {
            $(element).text("");
        }
        else {
            var date = moment(valueUnwrapped, "DD-MM-YYYY"); //new Date(Date.fromISO(valueUnwrapped));            
            $(element).val(moment(date).format(pattern));
        }
    }
}

ko.bindingHandlers.dialog = {
    init: function(element, valueAccessor) {
        $(element).modal({
            show: false
        });

        setTimeout(function() {
            var value = valueAccessor();
            if (ko.isObservable(value)) {
                $(element).on('hide.bs.modal', function() {
                    value(false);
                });
            }
        },0);

        

        // Update 13/07/2016
        // based on @Richard's finding,
        // don't need to destroy modal explicitly in latest bootstrap.
        // modal('destroy') doesn't exist in latest bootstrap.
        // ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
        //    $(element).modal("destroy");
        // });

    },
    update: function(element, valueAccessor) {
        var value = valueAccessor();
        if (ko.utils.unwrapObservable(value)) {
            $(element).modal('show');
        } else {
            $(element).modal('hide');
        }
    }
};



ko.bindingHandlers["realVisible"] = {
    init: function (element, valueAccessor) {
        var val = ko.utils.unwrapObservable(valueAccessor());
        if (val) {
            $(element).show();
        } else {
            $(element).hide();
        }
    },
    update: function (element, valueAccessor) {
        var val = ko.utils.unwrapObservable(valueAccessor());
        if (val) {
            $(element).show();
        } else {
            $(element).hide();
        }
    }
};

var windowURL = window.URL || window.webkitURL;
ko.bindingHandlers.file = {
    init: function (element, valueAccessor) {
        $(element).change(function () {
            var file = this.files[0];
            if (ko.isObservable(valueAccessor())) {
                valueAccessor()(file);
            }
        });
    },
    
    update: function (element, valueAccessor, allBindingsAccessor) {
        var file = ko.utils.unwrapObservable(valueAccessor());
        var bindings = allBindingsAccessor();
        
        if (bindings.imageBase64 && ko.isObservable(bindings.imageBase64)) {
            if (!file) {
               // bindings.imageBase64(null);
                //bindings.imageType(null);
            } else {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var result = e.target.result || {};
                    var resultParts = result.split(",");
                    if (resultParts.length ===2) {
//                        console.log(resultParts[1]);
//                        console.log(resultParts[0]);
                        bindings.imageBase64(resultParts[1]);
                        bindings.imageType(resultParts[0]);
                    }
                    
                    //Now update fileObjet, we do this last thing as implementation detail, it triggers post
                    if (bindings.fileObjectURL && ko.isObservable(bindings.fileObjectURL)) {
                        var oldUrl = bindings.fileObjectURL();
                        if (oldUrl) {
                            windowURL.revokeObjectURL(oldUrl);
                        }
                        bindings.fileObjectURL(file && windowURL.createObjectURL(file));
                    }
                };
                reader.readAsDataURL(file);
            }
        }
    }
};

//    update: function (element, valueAccessor, allBindingsAccessor) {
//        var file = ko.utils.unwrapObservable(valueAccessor());
//        var bindings = allBindingsAccessor();
//        
//        if (bindings.fileObjectURL && ko.isObservable(bindings.fileObjectURL)) {
//            var oldUrl = bindings.fileObjectURL();
//            if (oldUrl) {
//                windowURL.revokeObjectURL(oldUrl);
//            }
//            bindings.fileObjectURL(file && windowURL.createObjectURL(file));
//            
//            if (bindings.fileBinaryData && ko.isObservable(bindings.fileBinaryData)) {
//                if (!file) {
//                    bindings.fileBinaryData(null);
//                } else {
//                    var reader = new FileReader();
//                    reader.onload = function(e) {
//                        bindings.fileBinaryData(e.target.result);
//                    };
//                    reader.readAsArrayBuffer(file);
//                }
//            }

//        }
//    }
//};
ko.bindingHandlers.img = {
    update: function (element, valueAccessor) {
        //grab the value of the parameters, making sure to unwrap anything that could be observable
        var value = ko.utils.unwrapObservable(valueAccessor()),
            src = ko.utils.unwrapObservable(value.src),
            fallback = ko.utils.unwrapObservable(value.fallback),
            $element = $(element);

        //now set the src attribute to either the bound or the fallback value
        if (src !== "data:image/jpg;base64,null") {
            $element.attr("src", src);
        } else {
            $element.attr("src", fallback);
        }
    },
    init: function (element, valueAccessor) {
        var $element = $(element);
        var value = ko.utils.unwrapObservable(valueAccessor());
        var busy = ko.utils.unwrapObservable(value.busy);
        
        $element.attr("src", busy);

        //hook up error handling that will unwrap and set the fallback value
        $element.error(function () {
            

            $element.attr("src", busy);
        });
    },
};
