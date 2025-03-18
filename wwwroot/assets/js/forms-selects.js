/**
 * Selects & Tags
 */

'use strict';

$(function () {
  const selectPicker = $('.selectpicker'),
    ReciverId = $('#ReciverId'),
    CCId = $('#CCId'),
    select2Icons = $('.select2-icons');

  // Bootstrap Select
  // --------------------------------------------------------------------
  if (selectPicker.length) {
    selectPicker.selectpicker();
  }

  // Select2
  // --------------------------------------------------------------------

  // Default
  if (ReciverId.length) {
    ReciverId.each(function () {
      var $this = $(this);
      $this.wrap('<div class="position-relative"></div>').select2({
        placeholder: 'دریافت کنندگان (To) را انتخاب کنید',
        dropdownParent: $this.parent()
      });
    });
  }
  if (CCId.length) {
    CCId.each(function () {
      var $this = $(this);
      $this.wrap('<div class="position-relative"></div>').select2({
        placeholder: 'رونوشت (cc) خالی است',
        dropdownParent: $this.parent()
      });
    });
  }

  // Select2 Icons
  if (select2Icons.length) {
    // custom template to render icons
    function renderIcons(option) {
      if (!option.id) {
        return option.text;
      }
      var $icon = "<i class='" + $(option.element).data('icon') + " me-2'></i>" + option.text;

      return $icon;
    }
    select2Icons.wrap('<div class="position-relative"></div>').select2({
      templateResult: renderIcons,
      templateSelection: renderIcons,
      escapeMarkup: function (es) {
        return es;
      }
    });
  }
});
