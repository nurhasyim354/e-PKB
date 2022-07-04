app.factory('dialogService', function ($timeout, $q) {
    var factory = {};
    var _saveSuccess = function () {
        var def = $q.defer();
        swal({
            title: "Sukses!",
            text: "Data telah disimpan.",
            timer: 2000,
            type: "success",
            showConfirmButton: false
        }, function () {
            swal.close();
            def.resolve();
        });
        return def.promise;
    }

    var _confirmDialog = function (msg) {
        var def = $q.defer();
        swal({
            title: "Are you sure?",
            text: msg,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            closeOnConfirm: false
        },
            function (a) {
                if (a)
                    def.resolve(a);
                else
                    def.reject(false);
            });
        return def.promise;
    }
    var _errorDialog = function (msg) {
        swal('Oops!', msg, 'info');
    }
    var _infoSuccessDialog = function (msg) {
        swal('Pajak Sudah Dibayar!', msg, 'success');
    }

    factory.errorDialog = _errorDialog;
    factory.saveSuccess = _saveSuccess;
    factory.confirmDialog = _confirmDialog;
    return factory;
});

app.factory('appService', function ($http, dialogService, $location, $q) {
    var factory = {};
    var _loadOption = function () {
        try {
            var def = $q.defer();
            var param = {};
            $http.post('/api/epkb/getoptions', param).then(function (success) {
                factory.Option = success.data;
                def.resolve(success.data);
            }, function (error) {
                dialogService.error(error.data.Message);
                def.reject(error);
            });
            return def.promise;
        }
        catch (e) {
            console.log(e);
        }
    }
    var _loadUser = function (a) {
        var param1 = {};
        param1.id = a;
        var def = $q.defer();
        $http.post('/api/epkb/userdata', param1).then(function (success) {
            def.resolve(success);
        }, function (error) {
            def.reject(error);
        });
        return def.promise;
    }
    var _loadProvinces = function () {
        if (factory.Provinces && factory.Provinces.length > 0) return;
        $http.get('/api/epkb/provinces').then(function (success) {
            factory.Provinces = success.data;
        });
    }
    var _saveRegistration = function (a) {
        var def = $q.defer();
        if (!_validRegister(a)) {
            def.reject();
            return def.promise;
        }
        var param = a;
        param.UserName = a.UserProfile.Email;
        param.Password = a.Password;
        $http.post('/api/account/register', param).then(function (success) {
            def.resolve(success.data);
        }, function (error) {
            def.reject(error);
        });
        return def.promise;
    }

    var _saveData = function (a) {
        var def = $q.defer();
        try
        {
            var param = a;
            param.Vehicle.IdRegion = a.Vehicle.Region.Id;
            param.Vehicle.Province = a.Vehicle.Province.Name;
            param.UserProfile.TaxPayments = [];
            param.UserProfile.Vehicles = [];
            $http.post('/api/epkb/save', param).then(function (success) {
                def.resolve(success.data);
                if (success.data != 'EXIST')
                    $location.path('/step2/' + success.data);
            }, function (error) {
                def.reject(error);
            });
            return def.promise;
        }
        catch (e)
        {
            console.log(e);
            return def.promise;
        }
       
    }

    var _validRegister = function (a) {
        if (a.Password != a.ConfirmPassword)
        {
            dialogService.errorDialog("Pastikan password anda benar!");
            return false;
        }
        return true;
    }
    var _sendNotification = function (id, a) {
        var param = {};
        param.id = id;
        param.msg = a;
        $http.post('/api/epkb/sendmail', param).then(function (success) {
            swal('Sukses!', "Email terkirim!", 'success');
        });
    }

    factory.status_validateData = 'VALIDASI DATA';
    factory.status_waitForPayment = 'MENUNGGU PEMBAYARAN';
    factory.status_confirmPayment = 'KONFIRMASI PEMBAYARAN'
    factory.status_print = 'CETAK BUKTI';
    factory.status_upload = 'UNGGAH DOKUMEN SYARAT';
    factory.Option = {};
    factory.loadOption = _loadOption;
    factory.loadUser = _loadUser;
    factory.loadProvinces = _loadProvinces;
    factory.saveRegistration = _saveRegistration;
    factory.saveData = _saveData;
    factory.sendNotification = _sendNotification;
    return factory;
});