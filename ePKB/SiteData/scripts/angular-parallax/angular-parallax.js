'use strict';
angular.module('angular-parallax', [
]).directive('parallax', function ($window) {
    return {
        restrict: 'A',
        scope: {
            parallaxRatio: '@',
            parallaxVerticalOffset: '@',
            parallaxHorizontalOffset: '@',
        },
        link: function ($scope, elem, attrs) {
            var setPosition = function () {
                if (!$scope.parallaxHorizontalOffset) $scope.parallaxHorizontalOffset = '0';
                var calcValY = $window.pageYOffset * ($scope.parallaxRatio ? $scope.parallaxRatio : 1.1);
                if (calcValY <= $window.innerHeight) {
                    var topVal = (calcValY < $scope.parallaxVerticalOffset ? $scope.parallaxVerticalOffset : calcValY);
                    var hozVal = ($scope.parallaxHorizontalOffset.indexOf("%") === -1 ? $scope.parallaxHorizontalOffset + 'px' : $scope.parallaxHorizontalOffset);
                    elem.css('transform', 'translate(' + hozVal + ', ' + topVal + 'px)');
                }
            };

            setPosition();

            angular.element($window).bind("scroll", setPosition);
            angular.element($window).bind("touchmove", setPosition);
        }
    };
}).directive('parallaxBackground', function ($window) {
    return {
        restrict: 'A',
        scope: {
            parallaxRatio: '@',
            parallaxVerticalOffset: '@',
            parallaxCover: '@',
        },
        link: function ($scope, elem, attrs) {
            var setPosition = function () {
                var ratio = $scope.parallaxRatio ? $scope.parallaxRatio : 0.3;
                var elementPosition = 10 * elem.prop('offsetTop') / 100 * (ratio * 10);
                var verticalOffset = $scope.parallaxVerticalOffset ? elementPosition - $scope.parallaxVerticalOffset : elementPosition;
                var calcValY = $window.pageYOffset * ratio - verticalOffset;
                if ($scope.parallaxCover) {
                    elem.css('background-size', 'cover');
                } else {
                    var bgSize = elem.prop('offsetHeight') + ((window.innerHeight - elem.prop('offsetHeight')) / 100) * (ratio * 100);
                    elem.css('background-size', 'auto ' + bgSize + "px");
                }
                elem.css('background-position', "50% " + calcValY + "px");
            };

            angular.element($window).bind("resize", setPosition);
            angular.element($window).bind("scroll", setPosition);
            angular.element($window).bind("touchmove", setPosition);

            elem.ready(function () {
                setPosition();
            })
        }
    };
});