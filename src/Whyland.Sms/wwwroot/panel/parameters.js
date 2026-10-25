// Parameters editor for the SMS template form.
// The MVC model binder needs contiguous Parameters[0..n] indices, so every add or remove
// rewrites the names of the surviving rows instead of trusting the DOM order.
(function () {
  var list = document.getElementById('parameters');
  var addButton = document.getElementById('add-parameter');
  var rowTemplate = document.getElementById('parameter-row-template');

  if (!list || !addButton || !rowTemplate) {
    return;
  }

  function reindex() {
    Array.prototype.forEach.call(list.children, function (row, index) {
      Array.prototype.forEach.call(row.querySelectorAll('[data-field]'), function (input) {
        input.name = 'Parameters[' + index + '].' + input.dataset.field;
      });
    });
  }

  function addRow() {
    list.appendChild(rowTemplate.content.cloneNode(true));
    reindex();
    var added = list.lastElementChild.querySelector('[data-field="Name"]');
    if (added) {
      added.focus();
    }
  }

  addButton.addEventListener('click', addRow);

  list.addEventListener('click', function (event) {
    var button = event.target.closest('[data-remove]');
    if (!button) {
      return;
    }

    // A template must declare at least one parameter, so the last row stays.
    if (list.children.length > 1) {
      button.closest('[data-row]').remove();
      reindex();
    }
  });
})();
