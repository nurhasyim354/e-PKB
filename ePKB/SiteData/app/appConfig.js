//MODULE
var app = angular.module('app',
    ['ngRoute', 'ngAnimate', 'ngTouch', 'ngSanitize', 'ngCookies',
        'ngProgressLite', 
        'flow', 'moment-picker', 'angularMoment', 'angular-linq',
        'ngHandsontable','angularUtils.directives.dirPagination'
    ]);

//CONSTANT VALUE
app.constant('constant', {
    ver: 0.5,
    tinyMCEConfig: {
        plugins: 'link image code table',
        valid_elements: '*[*]',
        //extended_valid_elements: 'i[*],' + " section[*],",
        content_css: ['/bundle/bundle.min.css', '/bundle/guest.min.css', '/bundle/admin.min.css'],
        //valid_children: "+body[style]"
    },
    dateFormat: 'DD MMM YYYY',
    sortDateFormat: 'MMM, DD YYYY'
});

app.config(['$qProvider', function ($qProvider) {
    $qProvider.errorOnUnhandledRejections(false);
}]);