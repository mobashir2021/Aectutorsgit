
var ValidateAdminPasswordViewModel = function () {

    var self = this;

    self.CallBack = ko.observable(null);
    self.Title = ko.observable(null);
    self.ValidatePasswordUrl = ko.observable(null);
    self.AdminPassword = ko.observable(null);
    self.IsAdminDialogOpen = ko.observable(false);
    self.showInvalidAdminPasswordMessage = ko.observable(false);

    self.ValidateAdminPassword = function () {

        $.post(self.ValidatePasswordUrl(), { password: self.AdminPassword() }, function (returnedData) {
            if (returnedData.IsMatch) {
                //close dialog
                self.IsAdminDialogOpen(false);

                //callback parent logic
                ko.unwrap(self.CallBack())();
                
            } else {
                //
                self.showInvalidAdminPasswordMessage(true);
            }
        });
    }

    self.Show =function(url, callback)
    {
        self.AdminPassword("");
        self.CallBack(callback);
        self.showInvalidAdminPasswordMessage(false);
        self.ValidatePasswordUrl(url);
        self.IsAdminDialogOpen(true);
    }

    self.CloseAdminDialog = function () {
        self.IsAdminDialogOpen(false);
    }
};

//ValidatePasswordViewModel.prototype.ValidateAdminPassword = function () {

//    $.post(this.ValidatePasswordUrl(), { password: this.AdminPassword() }, function (returnedData) {
//        if (returnedData.IsMatch) {
//            //close dialog
//            this.IsAdminDialogOpen(false);

//            //callback parent logic
//        } else {
//            //
//            this.showInvalidAdminPasswordMessage(true);
//        }
//    });
//}

ValidateAdminPasswordViewModel.prototype.dispose = function () {
    this.CallBackURL.dispose();
    this.ValidatePasswordUrl.dispose();
    this.AdminPassword.dispose();
    this.IsAdminDialogOpen.dispose();
    this.showInvalidAdminPasswordMessage.dispose();
}
