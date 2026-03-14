(function () {
    'use strict';
    
    angular.module('app', ['ngSanitize']);
    angular.module('app').filter('brTextFilter', ['$sce', function ($sce) {        
        return function (text) {
            return $sce.trustAsHtml(text ? text.replace(/\n/g, '<br>') : text);
            return text ? text.replace(/\n/g, '<br>') : text;
        };
    }]);
    angular.module('app').filter('numberWithCommas', function () {
        return function (input) {
            if (isNaN(input)) {
                return input;
            }
            var x = input.toString().split('.');
            var x1 = x[0];
            var x2 = x.length > 1 ? '.' + x[1] : '';
            var lastThree = x1.substring(x1.length - 3);
            var otherNumbers = x1.substring(0, x1.length - 3);
            if (otherNumbers != '') {
                lastThree = ',' + lastThree;
            }
            var formattedNumber = otherNumbers.replace(/\B(?=(\d{2})+(?!\d))/g, ",") + lastThree + x2;
            return formattedNumber;
            //return input.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        };
    });
    angular.module('app').filter('dateDisplay', function () {
            return function (input) {
                if (!input) return '';
                var date = new Date(input);
                var options = { day: '2-digit', month: '2-digit', year: 'numeric' };
                return date.toLocaleDateString('en-GB', options); // 'en-GB' for 'dd-MM-yyyy' format
            };
    });

    angular.module('app').filter('dateTimeDisplay', function () {
        return function (input) {
            if (!input) return '';
            var date = new Date(input);
            var options = { day: '2-digit', month: '2-digit', year: 'numeric' };
            var datePart = date.toLocaleDateString('en-GB', options); // 'dd-MM-yyyy'
            var hours = date.getHours().toString().padStart(2, '0');
            var minutes = date.getMinutes().toString().padStart(2, '0');
            var seconds = date.getSeconds().toString().padStart(2, '0');
            var timePart = hours + ':' + minutes + '.' + seconds;
            return datePart + ' ' + timePart;
        };
    });
})();