var ConfirmationViewModel= function () {

    var self = this;

    self.CallBack = ko.observable(null);
    //self.Title = ko.observable(null);
    self.Text = ko.observable(null);

    self.ShowConfirmationDialog = ko.observable(false);
    

    self.ExecuteAction = function (e) {
        // ko.unwrap(self.CallBack())();
        if (self.CallBack() !== null) {
            self.ShowConfirmationDialog(false);
            ko.unwrap(self.CallBack())();
            //self.CallBack()();
        }
        //alert('Execute clicked');
    };

    self.Show = function (text,callback) {
        self.CallBack(callback);
        self.Text(text);
        self.ShowConfirmationDialog(true);
    };

    self.CloseDialog = function () {
        self.ShowConfirmationDialog(false);
    };

};

ConfirmationViewModel.prototype.dispose = function () {

    self.ShowConfirmationDialog.dispose();
    self.CallBack.dispose();
    self.Text.dispose();
}