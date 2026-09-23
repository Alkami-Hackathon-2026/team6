(function () {
    function getBuilderElements(root) {
        return {
            panel: root,
            name: root.querySelector('[data-builder-name]'),
            status: root.querySelector('[data-builder-status]'),
            primary: [
                root.querySelector('[data-builder-step-primary="0"]'),
                root.querySelector('[data-builder-step-primary="1"]'),
                root.querySelector('[data-builder-step-primary="2"]')
            ],
            secondary: [
                root.querySelector('[data-builder-step-secondary="0"]'),
                root.querySelector('[data-builder-step-secondary="1"]'),
                root.querySelector('[data-builder-step-secondary="2"]')
            ]
        };
    }

    function setTextIfPresent(element, value) {
        if (element && value) {
            element.textContent = value;
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var builderRoot = document.querySelector('[data-rule-builder]');
        if (!builderRoot) {
            return;
        }

        var builder = getBuilderElements(builderRoot);
        var promptInput = document.querySelector('[data-rule-prompt]');
        var openButtons = document.querySelectorAll('[data-open-rule-builder]');
        var closeButton = document.querySelector('[data-close-rule-builder]');
        var activateButton = document.querySelector('[data-activate-rule]');
        var naturalLanguageForm = document.querySelector('[data-natural-language-form]');
        var defaultName = builder.name ? builder.name.textContent : 'Payday Vacation Saver';

        function openBuilder(details) {
            if (details && details.name) {
                builder.name.textContent = details.name;
            }

            setTextIfPresent(builder.primary[0], details && details.when);
            setTextIfPresent(builder.secondary[0], details && details.whenDetail);
            setTextIfPresent(builder.primary[1], details && details.ifText);
            setTextIfPresent(builder.secondary[1], details && details.ifDetail);
            setTextIfPresent(builder.primary[2], details && details.thenText);
            setTextIfPresent(builder.secondary[2], details && details.thenDetail);

            if (builder.status) {
                builder.status.textContent = '';
            }

            builder.panel.classList.remove('mrm-hidden');
            builder.panel.setAttribute('aria-hidden', 'false');
        }

        function closeBuilder() {
            builder.panel.classList.add('mrm-hidden');
            builder.panel.setAttribute('aria-hidden', 'true');
        }

        if (naturalLanguageForm) {
            naturalLanguageForm.addEventListener('submit', function (event) {
                event.preventDefault();
                var promptValue = promptInput ? promptInput.value.trim() : '';
                openBuilder({ name: promptValue || defaultName });
            });
        }

        Array.prototype.forEach.call(openButtons, function (button) {
            button.addEventListener('click', function () {
                var details = {
                    name: button.getAttribute('data-builder-name') || (promptInput && promptInput.value.trim()) || defaultName,
                    when: button.getAttribute('data-builder-when'),
                    whenDetail: button.getAttribute('data-builder-when-detail'),
                    ifText: button.getAttribute('data-builder-if'),
                    ifDetail: button.getAttribute('data-builder-if-detail'),
                    thenText: button.getAttribute('data-builder-then'),
                    thenDetail: button.getAttribute('data-builder-then-detail')
                };

                openBuilder(details);
            });
        });

        if (closeButton) {
            closeButton.addEventListener('click', function () {
                closeBuilder();
            });
        }

        if (activateButton) {
            activateButton.addEventListener('click', function () {
                if (builder.status) {
                    builder.status.textContent = 'Rule activated (mock).';
                }
            });
        }

        var ruleToggles = document.querySelectorAll('[data-rule-toggle]');
        Array.prototype.forEach.call(ruleToggles, function (toggle) {
            toggle.addEventListener('change', function () {
                var ruleItem = toggle.closest('[data-rule-item]');
                if (!ruleItem) {
                    return;
                }

                var status = ruleItem.querySelector('[data-rule-status]');
                var isActive = toggle.checked;
                ruleItem.setAttribute('data-rule-state', isActive ? 'active' : 'paused');
                if (status) {
                    status.textContent = isActive ? 'Active' : 'Paused';
                }
            });
        });
    });
})();
