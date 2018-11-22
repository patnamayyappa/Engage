$('.entity-grid.entitylist').on('focus', '[data-attribute=msdyn_invitationlink] a', function () {
    $(this).attr('target', '_blank');
});