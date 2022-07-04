//VALIDATE FORM
app.run(function ($rootScope) {
    $rootScope.validateForm = function (a) {
        if (a.$invalid) {
            swal("Validation error", "Please fill all required form with a proper value", "warning");
            return false;
        } else {
            return true;
        }
    }
});

//=============================FILTERS=============================
//TRUST HTML
app.filter('trusted', function ($sce) {
    return function (text) {
        return $sce.trustAsHtml(text);
    };
});

//=============================SERVICES=============================
//UPLOAD SERVICE
app.factory('uploadService', function ($http, $q) {
    var factory = {};
    var _upload = function (a, dir, filePrefix) {

        console.log(dir, filePrefix);

        var deferred = $q.defer();
        var fd = new FormData();
        fd.append('dir', dir);
        if (filePrefix)
            fd.append('prefix', filePrefix);
        for (var i = 0; i < a.length; i++) {
            var fileObj = a[i];
            fileObj instanceof window.File && fd.append('file-' + i, fileObj);
        }
        $http.post('/api/file/upload', fd, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        }).then(function (success) {
            deferred.resolve(success);
        }, function (error) {
            deferred.reject(error)
        });
        return deferred.promise;
    };
    factory.upload = _upload;
    return factory;
});

//===========================GLOBAL FUNCTIONS============================
//GET RANDOM STRING
function GetRandom() {
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < 46; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    return text;
}
