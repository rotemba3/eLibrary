
(function ($) {
    "use strict";


    /*==================================================================
    [ Focus Contact2 ]*/
    $('.input100').each(function () {
        $(this).on('blur', function () {
            if ($(this).val().trim() != "") {
                $(this).addClass('has-val');
            }
            else {
                $(this).removeClass('has-val');
            }
        })
    })


    /*==================================================================
    [ Validate ]*/
    var input = $('.validate-input .input100');

    $('.validate-form').on('submit', function () {
        var check = true;

        for (var i = 0; i < input.length; i++) {
            if (validate(input[i]) == false) {
                showValidate(input[i]);
                check = false;
            }
        }

        return check;
    });


    $('.validate-form .input100').each(function () {
        $(this).focus(function () {
            hideValidate(this);
        });
    });

    function validate(input) {
        if ($(input).attr('name') == 'username') {
            // בדיקה ששדה שם המשתמש מכיל רק אותיות, מספרים, או קו תחתון (לפחות 3 תווים)
            if ($(input).val().trim().match(/^[a-zA-Z0-9_]{3,}$/) == null) {
                return false;
            }
        }
        else {
            if ($(input).val().trim() == '') {
                return false;
            }
        }
        return true;
    }

    function showValidate(input) {
        var thisAlert = $(input).parent();

        $(thisAlert).addClass('alert-validate');
        // הצגת הודעת שגיאה מותאמת
        if (!$(thisAlert).find('.error-message').length) {
            let errorMessage = '';
            if ($(input).attr('name') == 'username') {
                errorMessage = 'Username must be at least 3 characters long and contain only letters, numbers, or underscores.';
            } else if ($(input).attr('name') == 'pass') {
                errorMessage = 'Password cannot be empty.';
            } else {
                errorMessage = 'This field is required.';
            }

            $(thisAlert).append('<span class="error-message">' + errorMessage + '</span>');
        }
    }

    function hideValidate(input) {
        var thisAlert = $(input).parent();

        $(thisAlert).removeClass('alert-validate');
        // הסרת הודעת השגיאה
        $(thisAlert).find('.error-message').remove();
    }


})(jQuery);