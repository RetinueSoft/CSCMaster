(function () {
    'use strict';

    angular.module('app').service('EntrolmentService', EntrolmentService);
    EntrolmentService.$inject = ['server'];

    function EntrolmentService(server) {   
        this.featchMember = featchMember;
        this.uploadFile = uploadFile;
        this.getDistrictEntrolment = getDistrictEntrolment;
        this.getDistrictMemberEntrolment = getDistrictMemberEntrolment;
        this.getMemberEntrolment = getMemberEntrolment;

        function featchMember(stationID, sucessFunc, failureFunc) {
            server.Get('/Enrolments/GetMember?stationID=' + stationID, sucessFunc, failureFunc);
        }
        function uploadFile(input, sucessFunc, failureFunc) {
            server.Post('/Enrolments/UploadEnrolmentData', input , sucessFunc, failureFunc);
        }
        function getDistrictEntrolment(input, sucessFunc, failureFunc) {
            server.Post('/Enrolments/MyDistricts', input, sucessFunc, failureFunc);
        }
        function getDistrictMemberEntrolment(input, sucessFunc, failureFunc) {
            server.Post('/Enrolments/MyDistrictMembers', input, sucessFunc, failureFunc);
        }
        function getMemberEntrolment(input, sucessFunc, failureFunc) {
            server.Post('/Enrolments/MyMemberEnrolments', input, sucessFunc, failureFunc);
        }
    }
})();