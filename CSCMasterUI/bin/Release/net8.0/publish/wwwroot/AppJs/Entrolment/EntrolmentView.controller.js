var myModule = angular.module('app');

myModule.controller('EntrolmentViewController', ['$scope', 'EntrolmentService', 'PopupModalService', EntrolmentViewController]);
myModule.filter('sumBy', function () {
    return function (data, key) {
        if (!angular.isArray(data) || !key) return 0;

        var sum = 0;
        angular.forEach(data, function (v) {
            console.log(v);
            sum += parseFloat(v[key]) || 0;
        });

        return sum;
    };
});
function EntrolmentViewController(binder, service, popupModal) {
    binder.input = { fromDate: new Date(), toDate: new Date() };
    binder.districts = [];
    binder.members = [];
    binder.memberEntrolments = [];
    binder.tableApi = null;
    binder.listItems = listItems;
    binder.loadDistrict = loadDistrict;
    binder.loadMember = loadMember;
    binder.loadMemberEntrolment = loadMemberEntrolment;

    binder.onPagedTableInit = function (api) {
        binder.tableApi = api;
    };
    function listItems(pageNumber, successFun) {
        //pageNumber, 10,
        service.getDistrictEntrolment(binder.input, function (result) {
            //result.pagedList.forEach(function (item) {
            //    item.roleStr = binder.roles.find(x => x.id === item.role).description;
            //});
            successFun(result);
        });
    }

    function loadDistrict() {
        service.getDistrictEntrolment(binder.input, function (result) {
            binder.districts = result;
        });
    }

    function loadMember(district) {
        district.memExp = !district.memExp;

        var prevInp = binder._prevMemberInput || {};
        var inp = JSON.parse(JSON.stringify(binder.input));
        inp.districtId = district.id;
        
        if (JSON.stringify(prevInp) === JSON.stringify(inp))
            return;

        if (!district.memExp)
            return;

        binder._prevMemberInput = inp;

        service.getDistrictMemberEntrolment(inp, function (result) {
            district.members = result;
            binder.input.memExp = !binder.input.memExp;
        });
    }

    function loadMemberEntrolment(member) {
        member.detExp = !member.detExp;

        var prevInp = binder._prevMemberEntrolmentInput || {};
        var inp = JSON.parse(JSON.stringify(binder.input));
        inp.memberId = member.id;

        if (JSON.stringify(prevInp) === JSON.stringify(inp))
            return;

        if (!member.detExp)
            return;

        binder._prevMemberEntrolmentInput = inp;


        service.getMemberEntrolment(inp, function (result) {
            member.memberEntrolments = result;
        });
    }
}


