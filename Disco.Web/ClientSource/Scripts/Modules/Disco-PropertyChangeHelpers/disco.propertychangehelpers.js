if (!document.DiscoFunctions) {
        document.DiscoFunctions = {};
    }
    if (!document.DiscoFunctions.PropertyChangeHelper){
        document.DiscoFunctions.PropertyValue = function(PropertyField){
            if (PropertyField[0].nodeName.toLowerCase()=='input' && PropertyField.attr('type')=='checkbox'){
                return PropertyField.is(':checked');
            }
            return PropertyField.val();
        };
        document.DiscoFunctions.PropertyChangeHelper = function (PropertyField, FieldWatermark, UpdateUrl, UpdatePropertyName) {
            var fieldValue = document.DiscoFunctions.PropertyValue(PropertyField);
            var fieldChangeToken = null;
            var $ajaxSave = PropertyField.nextAll('.ajaxSave').first();
            var $ajaxLoading = PropertyField.nextAll('.ajaxLoading').first();
            var fieldChangeFunction = function(){
                    $ajaxSave.hide();
                    var changedValue = document.DiscoFunctions.PropertyValue(PropertyField);
                    if (fieldValue != changedValue){
                        fieldValue = changedValue;
                        if (fieldChangeToken)
                            window.clearTimeout(fieldChangeToken);
                        fieldChangeToken = window.setTimeout(function(){
                            $ajaxLoading.show();
                            var data = {};
                            data[UpdatePropertyName] = fieldValue;
                            $.getJSON(UpdateUrl, data, function (response, result) {
                                if (result != 'success' || response != 'OK') {
                                    alert('Unable to change property "' + UpdatePropertyName + '":\n' + response);
                                    $ajaxLoading.hide();
                                } else {
                                    $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                                }
                            })
                            fieldChangeToken = null;
                        }, 500);
                    };
                }
            if (PropertyField[0].nodeName.toLowerCase()=='input' && PropertyField.attr('type')=='checkbox'){
                PropertyField.click(fieldChangeFunction);
            }else{
                PropertyField.change(fieldChangeFunction);
            }
            // For Input Text Boxes
            if (PropertyField[0].nodeName.toLowerCase()=='input' && PropertyField.attr('type')=='text'){
                PropertyField.keydown(function(e){
                    $ajaxSave.show();
                    if (e.which == 13) {
                        $(this).blur();
                    }
                })
                .watermark(FieldWatermark)
                .blur(function () {
                    $ajaxSave.hide();
                }).focus(function(){
                    $(this).select();
                });
            }
            // For TextAreas
            if (PropertyField[0].nodeName.toLowerCase()=='textarea'){
                PropertyField.keydown(function(){
                    $ajaxSave.show();
                })
            }
        }
    };
    if (!document.DiscoFunctions.DateChangeUserHelper){
        document.DiscoFunctions.DateChangeUserHelper = function (DateField, UserField, DateFieldWatermark, UpdateUrl, UpdatePropertyName, minDate, dateOnly) {
            var dateFieldValue = DateField.val();
            var dateFieldChangeToken = null;
            var $ajaxLoading = UserField.next('.ajaxLoading');
            DateField
                .watermark(DateFieldWatermark)
                .change(function(){
                    var dateText = DateField.val();
                    if (dateFieldValue.toLowerCase() != dateText.toLowerCase()){
                        dateFieldValue = dateText;
                        if (dateFieldChangeToken)
                            window.clearTimeout(dateFieldChangeToken);
                        dateFieldChangeToken = window.setTimeout(function(){
                            $ajaxLoading.show();
                            var data = {};
                            data[UpdatePropertyName] = dateFieldValue;
                            $.getJSON(UpdateUrl, data, function (response, result) {
                                if (result != 'success' || response.Result != 'OK') {
                                    alert('Unable to change Date:\n' + response);
                                    $ajaxLoading.hide();
                                } else {
                                    UserField.text('by ' + response.UserDescription);
                                    $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                                }
                            })
                            dateFieldChangeToken = null;
                        }, 500);
                    }
                }).focus(function(){
                    $(this).select();
                });

                if (dateOnly){
                    DateField.datepicker({
                        defaultDate: new Date(),
                        minDate: minDate,
                        changeYear: true,
                        changeMonth: true,
                        dateFormat: 'yy/mm/dd',
                        beforeShow: function(input, inst){
                            $input = $(input);
                            if (!$input.val()){
                                $input.datepicker('setDate', new Date());
                            }
                        }
                    });
                }else{
                    DateField.datetimepicker({
                        defaultDate: new Date(),
                        ampm: true,
                        minDate: minDate,
                        changeYear: true,
                        changeMonth: true,
                        dateFormat: 'yy/mm/dd',
                        beforeShow: function(input, inst){
                            $input = $(input);
                            if (!$input.val()){
                                $input.datetimepicker('setDate', new Date());
                            }
                        }
                    });
                }

        }
    };
    if (!document.DiscoFunctions.DateChangeHelper){
        document.DiscoFunctions.DateChangeHelper = function (DateField, DateFieldWatermark, UpdateUrl, UpdatePropertyName, minDate, dateOnly) {
            var dateFieldValue = DateField.val();
            var dateFieldChangeToken = null;
            var $ajaxLoading = DateField.next('.ajaxLoading');
            DateField
                .watermark(DateFieldWatermark)
                .change(function(){
                var dateText = DateField.val();
                    if (dateFieldValue.toLowerCase() != dateText.toLowerCase()){
                        dateFieldValue = dateText;
                        if (dateFieldChangeToken)
                            window.clearTimeout(dateFieldChangeToken);
                        dateFieldChangeToken = window.setTimeout(function(){
                            $ajaxLoading.show();
                            var data = {};
                            data[UpdatePropertyName] = dateFieldValue;
                            $.getJSON(UpdateUrl, data, function (response, result) {
                                if (result != 'success' || response != 'OK') {
                                    alert('Unable to change Date:\n' + response);
                                    $ajaxLoading.hide();
                                } else {
                                    $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                                }
                            })
                            dateFieldChangeToken = null;
                        }, 500);
                    }
                }).focus(function(){
                    $(this).select();
                });

                if (dateOnly){
                    DateField.datepicker({
                        defaultDate: new Date(),
                        minDate: minDate,
                        changeYear: true,
                        changeMonth: true,
                        dateFormat: 'yy/mm/dd',
                        beforeShow: function(input, inst){
                            $input = $(input);
                            if (!$input.val()){
                                $input.datepicker('setDate', new Date());
                            }
                        }
                    });
                }else{
                    DateField.datetimepicker({
                        defaultDate: new Date(),
                        ampm: true,
                        minDate: minDate,
                        changeYear: true,
                        changeMonth: true,
                        dateFormat: 'yy/mm/dd',
                        beforeShow: function(input, inst){
                            $input = $(input);
                            if (!$input.val()){
                                $input.datetimepicker('setDate', new Date());
                            }
                        }
                    });
                }

        };
    }