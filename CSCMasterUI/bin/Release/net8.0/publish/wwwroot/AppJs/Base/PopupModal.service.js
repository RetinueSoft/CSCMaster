var myModule = angular.module('app');
myModule.service('PopupModalService', function ($compile, $rootScope) {

    this.show = function (options) {

        var scope = $rootScope.$new(true);

        scope.title = options.title || "Notification";
        scope.message = options.message || "";
        scope.type = options.type || "success";
        // success | error | warning | info

        // Dynamic styling based on type
        var headerClass = {
            success: "bg-success text-white",
            error: "bg-danger text-white",
            warning: "bg-warning text-dark",
            info: "bg-info text-white"
        }[scope.type];

        var buttonClass = {
            success: "btn-success",
            error: "btn-danger",
            warning: "btn-warning",
            info: "btn-info"
        }[scope.type];

        var modalHtml = `
            <div class="modal fade" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">

                        <div class="modal-header ${headerClass}">
                            <h5 class="modal-title">{{title}}</h5>
                            <button type="button" class="close close-btn">
                                &times;
                            </button>
                        </div>

                        <div class="modal-body">
                            <p>{{message}}</p>
                        </div>

                        <div class="modal-footer">
                            <button type="button"
                                    class="btn ${buttonClass} ok-btn">
                                OK
                            </button>
                        </div>

                    </div>
                </div>
            </div>
        `;

        var element = $compile(modalHtml)(scope);
        angular.element(document.body).append(element);

        var modalElement = $(element);

        modalElement.modal({
            backdrop: 'static',
            keyboard: true
        });

        modalElement.modal('show');

        // Manual close handler
        modalElement.find('.ok-btn, .close-btn').on('click', function () {
            modalElement.modal('hide');
        });

        modalElement.on('hidden.bs.modal', function () {
            modalElement.remove();
            scope.$destroy();
        });
    };

    // Helper shortcuts
    this.success = function (message) {
        this.show({ title: "Success", message: message, type: "success" });
    };

    this.error = function (message) {
        this.show({ title: "Error", message: message, type: "error" });
    };

    this.warning = function (message) {
        this.show({ title: "Warning", message: message, type: "warning" });
    };

    this.info = function (message) {
        this.show({ title: "Information", message: message, type: "info" });
    };

});