(function () {
    'use strict';

    function initKeyboard(root) {
        const targetId = root.dataset.targetId;
        const input = document.getElementById(targetId);
        if (!input) return;

        let lastSelectionStart = input.selectionStart ?? input.value.length;
        let lastSelectionEnd = input.selectionEnd ?? input.value.length;

        function rememberSelection() {
            lastSelectionStart = input.selectionStart ?? input.value.length;
            lastSelectionEnd = input.selectionEnd ?? lastSelectionStart;
        }

        function setSelection(start, end) {
            input.focus();
            const safeStart = Math.max(0, Math.min(start, input.value.length));
            const safeEnd = Math.max(safeStart, Math.min(end, input.value.length));
            input.setSelectionRange(safeStart, safeEnd);
            rememberSelection();
        }

        function notifyInput() {
            input.dispatchEvent(new Event('input', { bubbles: true }));
            input.dispatchEvent(new Event('change', { bubbles: true }));
        }

        function replaceSelection(text, cursorOffset) {
            const start = input.selectionStart ?? lastSelectionStart;
            const end = input.selectionEnd ?? lastSelectionEnd;
            input.value = input.value.slice(0, start) + text + input.value.slice(end);
            const cursor = start + (cursorOffset ?? text.length);
            setSelection(cursor, cursor);
            notifyInput();
        }

        function insert(text) {
            replaceSelection(text, text.length);
        }

        function insertPair(open, close) {
            replaceSelection(open + close, open.length);
        }

        function wrap(functionName) {
            const start = input.selectionStart ?? lastSelectionStart;
            const end = input.selectionEnd ?? lastSelectionEnd;
            const selected = input.value.slice(start, end);
            const text = `${functionName}(${selected})`;
            replaceSelection(text, selected ? text.length : functionName.length + 1);
        }

        function square() {
            const start = input.selectionStart ?? lastSelectionStart;
            const end = input.selectionEnd ?? lastSelectionEnd;
            const selected = input.value.slice(start, end);

            if (!selected) {
                insert('^2');
                return;
            }

            const text = `(${selected})^2`;
            replaceSelection(text, text.length);
        }

        function backspace() {
            const start = input.selectionStart ?? lastSelectionStart;
            const end = input.selectionEnd ?? lastSelectionEnd;

            if (start !== end) {
                replaceSelection('', 0);
                return;
            }

            if (start <= 0) return;
            input.value = input.value.slice(0, start - 1) + input.value.slice(start);
            setSelection(start - 1, start - 1);
            notifyInput();
        }

        root.querySelectorAll('[data-insert]').forEach(button => {
            button.addEventListener('mousedown', event => event.preventDefault());
            button.addEventListener('click', () => insert(button.dataset.insert || ''));
        });

        root.querySelectorAll('[data-wrap]').forEach(button => {
            button.addEventListener('mousedown', event => event.preventDefault());
            button.addEventListener('click', () => wrap(button.dataset.wrap));
        });

        root.querySelector('[data-action="square"]')?.addEventListener('mousedown', event => event.preventDefault());
        root.querySelector('[data-action="square"]')?.addEventListener('click', square);

        root.querySelectorAll('[data-action="backspace"]').forEach(button => {
            button.addEventListener('mousedown', event => event.preventDefault());
            button.addEventListener('click', backspace);
        });

        root.querySelectorAll('[data-action="clear"]').forEach(button => {
            button.addEventListener('mousedown', event => event.preventDefault());
            button.addEventListener('click', () => {
                input.value = '';
                setSelection(0, 0);
                notifyInput();
            });
        });

        root.querySelector('[data-action="focus-end"]')?.addEventListener('mousedown', event => event.preventDefault());
        root.querySelector('[data-action="focus-end"]')?.addEventListener('click', () => {
            setSelection(input.value.length, input.value.length);
        });

        // Entering an opening parenthesis from the keyboard creates a matching pair
        // only when the physical input ends with '(' or when the keyboard button is used.
        const openParenthesis = root.querySelector('[data-insert="("]');
        if (openParenthesis) {
            openParenthesis.dataset.action = 'parentheses';
            delete openParenthesis.dataset.insert;
            openParenthesis.addEventListener('mousedown', event => event.preventDefault());
            openParenthesis.addEventListener('click', () => insertPair('(', ')'));
        }

        ['click', 'keyup', 'select', 'input', 'focus'].forEach(eventName => {
            input.addEventListener(eventName, rememberSelection);
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('.math-keyboard').forEach(initKeyboard);
    });
})();
