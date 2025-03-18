/**
 * Tagify
 */

'use strict';

(function () {
  // Basic
  //------------------------------------------------------
  const tagifyBasicEl = document.querySelector('#TagifyBasic');
  const TagifyBasic = new Tagify(tagifyBasicEl);

  // Read only
  //------------------------------------------------------
  const tagifyReadonlyEl = document.querySelector('#TagifyReadonly');
  const TagifyReadonly = new Tagify(tagifyReadonlyEl);

  // Custom list & inline suggestion
  //------------------------------------------------------
  const TagifyCustomInlineSuggestionEl = document.querySelector('#TagifyCustomInlineSuggestion');
  const TagifyCustomListSuggestionEl = document.querySelector('#TagifyCustomListSuggestion');

  const whitelist = [
    'A# .NET',
    'A# (Axiom)',
    'A-0 System',
    'A+',
    'A++',
    'ABAP',
    'ABC',
    'ABC ALGOL',
    'ABSET',
    'ABSYS',
    'ACC',
    'Accent',
    'Ace DASL',
    'ACL2',
    'Avicsoft',
    'ACT-III',
    'Action!',
    'ActionScript',
    'Ada',
    'Adenine',
    'Agda',
    'Agilent VEE',
    'Agora',
    'AIMMS',
    'Alef',
    'ALF',
    'ALGOL 58',
    'ALGOL 60',
    'ALGOL 68',
    'ALGOL W',
    'Alice',
    'Alma-0',
    'AmbientTalk',
    'Amiga E',
    'AMOS',
    'AMPL',
    'Apex (Salesforce.com)',
    'APL',
    'AppleScript',
    'Arc',
    'ARexx',
    'Argus',
    'AspectJ',
    'Assembly language',
    'ATS',
    'Ateji PX',
    'AutoHotkey',
    'Autocoder',
    'AutoIt',
    'AutoLISP / Visual LISP',
    'Averest',
    'AWK',
    'Axum',
    'Active Server Pages',
    'ASP.NET'
  ];
  // Inline
  let TagifyCustomInlineSuggestion = new Tagify(TagifyCustomInlineSuggestionEl, {
    whitelist: whitelist,
    maxTags: 10,
    dropdown: {
      maxItems: 20,
      classname: 'tags-inline',
      enabled: 0,
      closeOnSelect: false
    }
  });
  // List
  let TagifyCustomListSuggestion = new Tagify(TagifyCustomListSuggestionEl, {
    whitelist: whitelist,
    maxTags: 10,
    dropdown: {
      maxItems: 20,
      classname: '',
      enabled: 0,
      closeOnSelect: false
    }
  });

  // Users List suggestion
  //------------------------------------------------------
  const TagifyUserListEl = document.querySelector('#TagifyUserList');

  // ایجاد Tagify برای لیست to و cc  
  const tagifyTo = new Tagify(document.querySelector('#to'), {
    whitelist: [], // لیست کاربران تا از API دریافت شود  
    onInvalid: onTagInvalid // در صورت وارد کردن کاربر در هر دو لیست  
  });

  const tagifyCC = new Tagify(document.querySelector('#cc'), {
    whitelist: [], // لیست کاربران تا از API دریافت شود  
    onInvalid: onTagInvalid // در صورت وارد کردن کاربر در هر دو لیست  
  });

  

  // مدیریت ورود نادرست کاربر  
  function onTagInvalid(e) {
    const tag = e.detail.tag;

    // برای بررسی اینکه آیا تگ قبلاً در لیست دیگری وجود دارد یا خیر  
    const isInCC = tagifyCC.getTag(tag.value);
    const isInTo = tagifyTo.getTag(tag.value);

    if (isInCC || isInTo) {
      const msg = `${tag.value} is already added to ${isInCC ? 'CC' : 'To'}.`;
      alert(msg);
      // حذف تگ از لیست نادرست  
      if (isInTo) {
        tagifyTo.removeTag(tag);
      } else {
        tagifyCC.removeTag(tag);
      }
    }
  }

  loadUsers(); // بارگذاری کاربران  
  // ===========================

  function tagTemplate(tagData) {
    return `
    <tag title="${tagData.title || tagData.username}"
      contenteditable='false'
      spellcheck='false'
      tabIndex="-1"
      class="${this.settings.classNames.tag} ${tagData.class ? tagData.class : ''}"
      ${this.getAttributes(tagData)}
    >
      <x title='' class='tagify__tag__removeBtn' role='button' aria-label='remove tag'></x>
      <div>
        <div class='tagify__tag__avatar-wrap'>
          <img onerror="this.style.visibility='hidden'" src="${tagData.avatar}">
        </div>
        <span class='tagify__tag-text'>${tagData.name}</span>
      </div>
    </tag>
  `;
  }

  function suggestionItemTemplate(tagData) {
    return `
    <div ${this.getAttributes(tagData)}
      class='tagify__dropdown__item align-items-center ${tagData.class ? tagData.class : ''}'
      tabindex="0"
      role="option"
    >
      ${tagData.avatar
        ? `<div class='tagify__dropdown__item__avatar-wrap'>
          <img onerror="this.style.visibility='hidden'" src="${tagData.avatar}">
        </div>`
        : ''
      }
      <strong>${tagData.name}</strong>
      <span>${tagData.username}</span>
    </div>
  `;
  }

  // initialize Tagify on the above input node reference
  // let TagifyUserList = new Tagify(TagifyUserListEl, {
  //   tagTextProp: 'name', // very important since a custom template is used with this property as text. allows typing a "value" or a "name" to match input with whitelist
  //   enforceWhitelist: true,
  //   skipInvalid: true, // do not remporarily add invalid tags
  //   dropdown: {
  //     closeOnSelect: false,
  //     enabled: 0,
  //     classname: 'users-list',
  //     searchKeys: ['name', 'username'] // very important to set by which keys to search for suggesttions when typing
  //   },
  //   templates: {
  //     tag: tagTemplate,
  //     dropdownItem: suggestionItemTemplate
  //   },
  //   whitelist: usersList
  // });

  // TagifyUserList.on('dropdown:show dropdown:updated', onDropdownShow);
  // TagifyUserList.on('dropdown:select', onSelectSuggestion);

  // let addAllSuggestionsEl;

  // function onDropdownShow(e) {
  //   let dropdownContentEl = e.detail.tagify.DOM.dropdown.content;

  //   if (TagifyUserList.suggestedListItems.length > 1) {
  //     addAllSuggestionsEl = getAddAllSuggestionsEl();

  //     // insert "addAllSuggestionsEl" as the first element in the suggestions list
  //     dropdownContentEl.insertBefore(addAllSuggestionsEl, dropdownContentEl.firstChild);
  //   }
  // }

  // function onSelectSuggestion(e) {
  //   if (e.detail.elm == addAllSuggestionsEl) TagifyUserList.dropdown.selectAll.call(TagifyUserList);
  // }

  // // create an "add all" custom suggestion element every time the dropdown changes
  // function getAddAllSuggestionsEl() {
  //   // suggestions items should be based on "dropdownItem" template
  //   return TagifyUserList.parseTemplate('dropdownItem', [
  //     {
  //       class: 'addAll',
  //       name: 'افزودن همه',
  //       username:
  //         TagifyUserList.settings.whitelist.reduce(function (remainingSuggestions, item) {
  //           return TagifyUserList.isTagDuplicate(item.value) ? remainingSuggestions : remainingSuggestions + 1;
  //         }, 0) + ' کاربر'
  //     }
  //   ]);
  // }

  // Email List suggestion
  //------------------------------------------------------
  // generate random whitelist items (for the demo)
  // let randomStringsArr = Array.apply(null, Array(100)).map(function () {
  //   return (
  //     Array.apply(null, Array(~~(Math.random() * 10 + 3)))
  //       .map(function () {
  //         return String.fromCharCode(Math.random() * (123 - 97) + 97);
  //       })
  //       .join('') + '@gmail.com'
  //   );
  // });

  // const TagifyEmailListEl = document.querySelector('#TagifyEmailList'),
  //   TagifyEmailList = new Tagify(TagifyEmailListEl, {
  //     // email address validation (https://stackoverflow.com/a/46181/104380)
  //     pattern:
  //       /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/,
  //     whitelist: randomStringsArr,
  //     callbacks: {
  //       invalid: onInvalidTag
  //     },
  //     dropdown: {
  //       position: 'text',
  //       enabled: 1 // show suggestions dropdown after 1 typed character
  //     }
  //   }),
  //   button = TagifyEmailListEl.nextElementSibling; // "add new tag" action-button

  // button.addEventListener('click', onAddButtonClick);

  // function onAddButtonClick() {
  //   TagifyEmailList.addEmptyTag();
  // }

  // function onInvalidTag(e) {
  //   console.log('invalid', e.detail);
  // }
})();
