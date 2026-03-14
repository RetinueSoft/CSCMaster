var myModule = angular.module('app');

myModule.controller('UploadEntrolmentController', ['$scope', 'EntrolmentService', 'PopupModalService', UploadEntrolmentController]);

function UploadEntrolmentController(binder, service, popupModal) {
    var userData = GetValueFromCache("userData");
    binder.input = { stationId: userData && userData.stationId ? userData.stationId : null };
    binder.featchMember = featchMember;
    binder.uploadFile = uploadFile;
    
    function featchMember(stationId) {
        if (!stationId)
            return;
        binder.input.member = {};
        service.featchMember(stationId, function (result) {
            RemoveControlError("stationId");
            if (result == null)
                SetControlError("stationId", "Station Id not match"); validationStatus = false;
            binder.input.member = result;
        });
    };

    function uploadFile() {
        var formData = new FormData();
        formData.append("Member", JSON.stringify(binder.input.member));
        formData.append("File", binder.input.file);

        service.uploadFile(formData, function (result) {
            popupModal.success("File uploaded successfully!");
            binder.input.file = null;
            angular.element('#fileInput').val(null);
        });
    };

    featchMember(binder.input.stationId);
}
myModule.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {

            var model = $parse(attrs.fileModel);
            var modelSetter = model.assign;

            element.bind('change', function () {
                scope.$apply(function () {
                    modelSetter(scope, element[0].files[0]);
                });
            });
        }
    };
}]);

