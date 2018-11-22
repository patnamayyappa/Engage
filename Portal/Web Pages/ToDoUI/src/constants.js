export const uiStatuses = {
    incomplete: {value: 'Incomplete', pluginValue: 0,  translationKey: 'PortalToDos_Incomplete'},
    complete: {value: 'Complete', pluginValue: 1,  translationKey: 'PortalToDos_Complete'},
    canceled: {value: 'Canceled/Waived', pluginValue: 2, translationKey: 'PortalToDos_CanceledWaived'}
};

export const sortOrders = {
    newest: {value: 'newest', label: 'Newest', translationKey: 'PortalToDos_Newest'},
    dueDate: {value: 'dueDate', label: 'Due Date', translationKey: 'PortalToDos_DueDate'},
    category: {value: 'category', label: 'Category', translationKey: 'PortalToDos_Category'},
    successPlan: {value: 'successPlan', label: 'Success Plan', translationKey: 'PortalToDos_SuccessPlan'}
};

export const newestCategories = {
    isNew: 'New To-Dos',
    isExisting: 'Existing To-Dos',
    isUnread: -100
};

export const statuscodes = {
    incomplete: 1,
    markedAsComplete: 175490000,
    complete: 2,
    canceled: 175490001,
    waived: 175490002
};

export const allTranslations = [
    'PortalToDos_Incomplete',
    'PortalToDos_Complete',
    'PortalToDos_CanceledWaived',
    'PortalToDos_NewToDos',
    'PortalToDos_ExistingToDos',
    'PortalToDos_MyToDos',
    'PortalToDos_SortBy',
    'PortalToDos_HideOptionalToDos',
    'PortalToDos_Due',
    'PortalToDos_Overdue',
    'PortalToDos_Required',
    'PortalToDos_New',
    'PortalToDos_AssignedBy',
    'PortalToDos_RequestedBy',
    'PortalToDos_At',
    'PortalToDos_MarkComplete',
    'PortalToDos_CancelToDoTask',
    'PortalToDos_MarkIncomplete',
    'PortalToDos_Newest',
    'PortalToDos_DueDate',
    'PortalToDos_Category',
    'PortalToDos_SuccessPlan',
    'PortalToDos_Comments',
    'PortalToDos_CommentsOptional',
    'cancelButton',
    'okButton',
    'PortalToDos_Error',
    'PortalToDos_LoadToDosError',
    'PortalToDos_UpdateToDoStatusError',
    'PortalToDos_RefreshToDosError',
    'PortalToDos_AccessDeniedError',
    'PortalToDos_NA'
];