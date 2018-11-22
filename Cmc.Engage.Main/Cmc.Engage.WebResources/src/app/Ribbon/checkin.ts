module CampusManagement.ribbon.checkin {
  export function quickCreateCheckin() {
    Xrm.Navigation.openForm({
      entityName: 'msevtmgt_checkin',
      useQuickCreateForm: true
    }, null);
  }

}
