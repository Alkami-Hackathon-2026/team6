<template>
    <div>
        <header class="mar-bottom--xl">
            <h1 class="font-large-heading mar--0">My Money Rules</h1>
            <p class="font-body-1 color-subtle mar-top--md mar-bottom--0">
                Automate the smart money habits you already know you should have.
            </p>
        </header>

        <iris-notification
            class="mar-bottom--xl"
            kind="inline"
            message-type="success"
            :message="savedMessage"
            :show="showSaved"
            @notification-close="showSaved = false"></iris-notification>

        <div id="rule-builder" class="hero-row mar-bottom--xl">
            <section class="builder-panel base-surface-low-emphasis--border border-radius--md">
                <h2 class="font-small-heading mar-top--0 mar-bottom--lg">What do you want your money to do?</h2>

                <div class="builder-mode-tabs mar-bottom--lg">
                    <iris-button
                        :kind="builderMode === 'ai' ? 'highEmphasis' : 'lowEmphasis'"
                        icon-name="bolt"
                        text="Build a rule with AI"
                        @button-click="setBuilderMode('ai')"></iris-button>
                    <span class="font-body-2 color-subtle">or</span>
                    <button
                        class="builder-text-link font-body-2"
                        :class="{ 'builder-text-link--active': builderMode === 'steps' }"
                        type="button"
                        @click="setBuilderMode('steps')">build a rule step by step</button>
                </div>

                <div v-if="builderMode === 'ai'">
                    <div class="mar-bottom--lg">
                        <iris-textfield
                            ref="aiInput"
                            v-model="aiDescription"
                            label="Describe your rule in plain English"
                            helper-text="For example, whenever I spend more than $50 at a restaurant, move $10 to savings"
                            :maxlength="280"></iris-textfield>
                    </div>
                    <iris-button
                        icon-name="bolt"
                        loading-text="Parsing your rule..."
                        text="Generate rule"
                        :is-disabled="!aiDescription.trim() || aiLoading"
                        :is-loading="aiLoading"
                        @button-click="generateAiRule"></iris-button>

                    <div v-if="aiResult" class="ai-result-preview mar-top--lg">
                        <p class="font-overline color-subtle mar-top--0 mar-bottom--sm">We think you mean</p>
                        <p class="font-body-1 mar-top--0 mar-bottom--md">{{ describeRule(aiResult) }}</p>
                        <div class="button-row">
                            <iris-button text="Save this rule" @button-click="saveAiRule"></iris-button>
                            <iris-button kind="lowEmphasis" text="Edit in builder" @button-click="editAiResult"></iris-button>
                        </div>
                    </div>
                </div>

                <div v-else>
                    <div class="wit-grid mar-bottom--lg">
                        <div class="wit-col">
                            <div class="wit-col__header">
                                <iris-icon aria-hidden="true" name="calendar"></iris-icon>
                                <span class="font-overline">When</span>
                            </div>
                            <div class="mar-bottom--md">
                                <iris-select-dropdown
                                    v-model="draft.whenAccount"
                                    :items="accountOptions"
                                    :strings="{ label: 'Account to watch' }"></iris-select-dropdown>
                            </div>
                            <iris-textfield
                                v-model="draft.whenAmount"
                                helper-text="Watch for transactions of this amount or more"
                                is-optional
                                label="Transaction amount"></iris-textfield>
                        </div>

                        <div class="wit-col">
                            <div class="wit-col__header">
                                <iris-icon aria-hidden="true" name="settings-slider"></iris-icon>
                                <span class="font-overline">If</span>
                            </div>
                            <div class="mar-bottom--md">
                                <iris-select-dropdown
                                    v-model="draft.ifScenario"
                                    :items="scenarioOptions"
                                    :strings="{ label: 'Condition' }"></iris-select-dropdown>
                            </div>
                            <iris-textfield
                                v-model="draft.ifValue"
                                :helper-text="ifScenarioValue === 'specific_vendor' ? 'Partial match, for example, Starbucks' : 'Dollar threshold'"
                                :key="ifScenarioValue"
                                :label="ifScenarioValue === 'specific_vendor' ? 'Vendor name' : 'Amount ($)'"></iris-textfield>
                        </div>

                        <div class="wit-col">
                            <div class="wit-col__header">
                                <iris-icon aria-hidden="true" name="bolt"></iris-icon>
                                <span class="font-overline">Then</span>
                            </div>
                            <div class="mar-bottom--md">
                                <iris-select-dropdown
                                    v-model="draft.thenAction"
                                    :items="actionOptions"
                                    :strings="{ label: 'Action' }"></iris-select-dropdown>
                            </div>

                            <div v-if="thenActionValue === 'notify'">
                                <p class="font-caption color-subtle mar-top--0 mar-bottom--sm">Notify me at:</p>
                                <div v-for="contact in userContacts" :key="contact.id" class="mar-bottom--sm">
                                    <iris-checkbox
                                        :is-checked="draft.thenContacts.indexOf(contact.id) !== -1"
                                        :label="contact.label"
                                        @checkbox-change="toggleContact(contact.id, $event)"></iris-checkbox>
                                </div>
                            </div>

                            <div v-else>
                                <div class="mar-bottom--md">
                                    <iris-select-dropdown
                                        v-model="draft.thenTransferAccount"
                                        :items="savingsAccountOptions"
                                        :strings="{ label: 'Transfer to' }"></iris-select-dropdown>
                                </div>
                                <iris-textfield
                                    v-model="draft.thenTransferAmount"
                                    helper-text="Fixed amount to transfer each time"
                                    label="Amount ($)"></iris-textfield>
                            </div>
                        </div>
                    </div>

                    <div class="action-bar">
                        <p class="font-caption color-subtle mar--0">{{ stepPreview }}</p>
                        <iris-button
                            icon-name="add"
                            text="Create rule"
                            :is-disabled="!canSaveStep"
                            @button-click="saveStepRule"></iris-button>
                    </div>
                </div>
            </section>

            <aside class="stats-panel base-surface-low-emphasis--border border-radius--md">
                <h2 class="font-small-heading mar-top--0 mar-bottom--md">Your automations</h2>
                <div class="stat-item">
                    <span class="stat-number font-large-heading">${{ formatCurrency(stats.savedThisYear) }}</span>
                    <p class="font-body-2 mar-top--sm mar-bottom--sm">Saved this year</p>
                    <button class="stat-link font-caption" type="button" @click="openPanel('savings')">View details</button>
                </div>
                <div class="stat-item">
                    <span class="stat-number font-large-heading">{{ activeRuleCount }}</span>
                    <p class="font-body-2 mar-top--sm mar-bottom--sm">Active rules</p>
                    <button class="stat-link font-caption" type="button" @click="openPanel('rules')">View details</button>
                </div>
                <div class="stat-item">
                    <span class="stat-number font-large-heading">{{ stats.completedActions }}</span>
                    <p class="font-body-2 mar-top--sm mar-bottom--sm">Completed actions</p>
                    <button class="stat-link font-caption" type="button" @click="openPanel('actions')">View details</button>
                </div>
            </aside>
        </div>

        <section class="mar-bottom--xl">
            <h2 class="font-small-heading mar-top--0 mar-bottom--lg">Popular recipes</h2>
            <div class="recipe-grid">
                <button
                    v-for="recipe in recipes"
                    :key="recipe.id"
                    class="recipe-card"
                    :aria-label="'Use recipe: ' + recipe.title"
                    type="button"
                    @click="applyRecipe(recipe)">
                    <iris-icon class="mar-bottom--sm" :name="recipe.icon" aria-hidden="true"></iris-icon>
                    <p class="font-subtitle-1 mar-top--sm mar-bottom--sm">{{ recipe.title }}</p>
                    <p class="font-body-2 color-subtle mar--0">{{ recipe.description }}</p>
                </button>
            </div>
        </section>

        <section>
            <div class="rules-heading mar-bottom--lg">
                <h2 class="font-small-heading mar--0">
                    Your rules <span class="font-body-2 color-subtle">({{ rules.length }})</span>
                </h2>
                <iris-button icon-name="add" kind="mediumEmphasis" text="New rule" @button-click="newRule"></iris-button>
            </div>

            <p v-if="!rules.length" class="font-body-1 color-subtle empty-state">
                No rules yet. Create one above or pick a recipe to get started.
            </p>
            <ul v-else class="rule-list mar--0 pad--0">
                <li v-for="rule in rules" :key="rule.id" class="rule-list__item">
                    <iris-card class="rule-card">
                        <div class="rule-card__body">
                            <div class="rule-card__main width-min--0">
                                <p class="font-subtitle-1 mar-top--0 mar-bottom--sm text--truncate">{{ rule.name }}</p>
                                <p class="font-body-2 color-subtle mar-top--0 mar-bottom--sm">{{ describeRule(rule) }}</p>
                                <div class="rule-wit-badges">
                                    <span class="wit-badge"><iris-icon aria-hidden="true" name="calendar"></iris-icon><span class="font-caption">{{ whenText(rule) }}</span></span>
                                    <span class="wit-badge"><iris-icon aria-hidden="true" name="settings-slider"></iris-icon><span class="font-caption">{{ ifText(rule) }}</span></span>
                                    <span class="wit-badge"><iris-icon aria-hidden="true" name="bolt"></iris-icon><span class="font-caption">{{ thenText(rule) }}</span></span>
                                </div>
                            </div>
                            <div class="rule-card__meta">
                                <p class="font-caption color-subtle mar--0 text--right">
                                    YTD: {{ rule.ytdCount }} actions<br>
                                    <span v-if="rule.ytdAmount > 0">${{ formatCurrency(rule.ytdAmount) }}</span>
                                </p>
                                <div class="rule-card__actions">
                                    <iris-switch
                                        :aria-label="(rule.enabled ? 'Disable ' : 'Enable ') + rule.name"
                                        :is-selected="rule.enabled"
                                        @switch-change="toggleRule(rule, $event)"></iris-switch>
                                    <span class="font-caption color-subtle">{{ rule.enabled ? 'Active' : 'Inactive' }}</span>
                                    <iris-button
                                        :aria-label="'Delete ' + rule.name"
                                        icon-name="delete"
                                        kind="lowEmphasis"
                                        @button-click="removeRule(rule.id)"></iris-button>
                                </div>
                            </div>
                        </div>
                    </iris-card>
                </li>
            </ul>
        </section>

        <div v-if="slidePanel.open" class="slide-overlay" @click.self="closePanel">
            <section class="slide-panel" aria-modal="true" role="dialog" :aria-label="slidePanel.title">
                <div class="rules-heading mar-bottom--lg">
                    <h3 class="font-small-heading mar--0">{{ slidePanel.title }}</h3>
                    <iris-button aria-label="Close panel" icon-name="times" kind="lowEmphasis" @button-click="closePanel"></iris-button>
                </div>
                <div v-if="slidePanel.type === 'savings'">
                    <p class="font-large-heading color-brand mar-top--0">${{ formatCurrency(stats.savedThisYear) }}</p>
                    <p class="font-body-1 color-subtle">Total moved to savings by your rules this year.</p>
                </div>
                <div v-if="slidePanel.type === 'rules'">
                    <p v-for="rule in rules" :key="rule.id" class="stat-item font-body-2">{{ rule.name }}: {{ rule.enabled ? 'Active' : 'Inactive' }}</p>
                </div>
                <div v-if="slidePanel.type === 'actions'">
                    <p v-for="rule in rules" :key="rule.id" class="stat-item font-body-2">{{ rule.name }}: {{ rule.ytdCount }} actions</p>
                </div>
            </section>
        </div>
    </div>
</template>

<script lang="ts">
import Vue from 'vue';

function firstOf(value: string[] | string): string {
    return Array.isArray(value) ? value[0] : value;
}

function emptyDraft() {
    return {
        whenAccount: ['all'],
        whenAmount: '',
        ifScenario: ['balance_over'],
        ifValue: '',
        thenAction: ['notify'],
        thenContacts: [] as string[],
        thenTransferAccount: ['savings'],
        thenTransferAmount: '',
    };
}

export default Vue.extend({
    data() {
        return {
            builderMode: 'ai',
            aiDescription: '',
            aiLoading: false,
            aiResult: null as any,
            accountOptions: [
                { label: 'All accounts', value: 'all' },
                { label: 'Everyday Checking ••1847', value: 'checking' },
                { label: 'Rainy Day Savings ••2290', value: 'savings' },
                { label: 'Signature Credit Card ••0076', value: 'credit' },
            ],
            savingsAccountOptions: [
                { label: 'Rainy Day Savings ••2290', value: 'savings' },
                { label: 'Everyday Checking ••1847', value: 'checking' },
            ],
            scenarioOptions: [
                { label: 'Balance is over an amount', value: 'balance_over' },
                { label: 'I spend over an amount', value: 'spent_amount' },
                { label: 'Specific vendor', value: 'specific_vendor' },
            ],
            actionOptions: [
                { label: 'Notify me', value: 'notify' },
                { label: 'Transfer funds', value: 'transfer' },
            ],
            userContacts: [
                { id: 'email_1', label: 'member@mybank.example.com', type: 'email' },
                { id: 'sms_1', label: '512-555-0147 (text)', type: 'sms' },
            ],
            stats: { savedThisYear: 247.80, completedActions: 41 },
            draft: emptyDraft(),
            recipes: [
                { id: 'coffee_jar', title: 'Coffee jar', description: 'Every time you buy coffee, move the same amount to savings', icon: 'recurring', draft: { ...emptyDraft(), ifScenario: ['specific_vendor'], ifValue: 'Coffee', thenAction: ['transfer'] } },
                { id: 'pay_yourself', title: 'Pay yourself first', description: '10% of every paycheck moves automatically to savings', icon: 'dollar-circle', draft: { ...emptyDraft(), whenAccount: ['checking'], thenAction: ['transfer'] } },
                { id: 'sweep', title: 'Sweep extra cash', description: 'Move excess balance at month end to savings', icon: 'transfer', draft: { ...emptyDraft(), whenAccount: ['checking'], ifValue: '2000', thenAction: ['transfer'] } },
                { id: 'big_purchase', title: 'Big purchase alert', description: 'Get notified whenever you spend over $200', icon: 'alert', draft: { ...emptyDraft(), whenAmount: '200', ifScenario: ['spent_amount'], ifValue: '200' } },
            ],
            rules: [
                { id: 1, name: 'Coffee jar', enabled: true, whenAccount: 'all', whenAmount: '', ifScenario: 'specific_vendor', ifValue: 'Starbucks', thenAction: 'notify', thenContacts: ['email_1'], thenTransferAccount: '', thenTransferAmount: '', ytdCount: 23, ytdAmount: 0 },
                { id: 2, name: 'Big purchase alert', enabled: true, whenAccount: 'credit', whenAmount: '200', ifScenario: 'spent_amount', ifValue: '200', thenAction: 'notify', thenContacts: ['email_1', 'sms_1'], thenTransferAccount: '', thenTransferAmount: '', ytdCount: 4, ytdAmount: 0 },
                { id: 3, name: 'Pay yourself first', enabled: false, whenAccount: 'checking', whenAmount: '', ifScenario: 'balance_over', ifValue: '2000', thenAction: 'transfer', thenContacts: [], thenTransferAccount: 'savings', thenTransferAmount: '50', ytdCount: 14, ytdAmount: 185.30 },
            ],
            slidePanel: { open: false, type: '', title: '' },
            showSaved: false,
            savedMessage: '',
        };
    },
    computed: {
        ifScenarioValue(): string {
            return firstOf(this.draft.ifScenario);
        },
        thenActionValue(): string {
            return firstOf(this.draft.thenAction);
        },
        activeRuleCount(): number {
            return this.rules.filter((rule: any) => rule.enabled).length;
        },
        canSaveStep(): boolean {
            return this.draft.ifValue.trim().length > 0 &&
                (this.thenActionValue === 'notify'
                    ? this.draft.thenContacts.length > 0
                    : this.draft.thenTransferAmount.trim().length > 0);
        },
        stepPreview(): string {
            if (!this.canSaveStep) {
                return 'Complete the If and Then fields above to preview your rule.';
            }

            return 'When ' + this.labelFor(this.accountOptions, this.draft.whenAccount) +
                ', if ' + this.scenarioText(this.ifScenarioValue, this.draft.ifValue) +
                ', then ' + this.actionText(this.thenActionValue, this.draft);
        },
    },
    methods: {
        setBuilderMode(mode: string): void {
            this.builderMode = mode;
            this.aiResult = null;
        },
        generateAiRule(): void {
            if (!this.aiDescription.trim() || this.aiLoading) {
                return;
            }

            this.aiLoading = true;
            this.aiResult = null;
            window.setTimeout(() => {
                this.aiLoading = false;
                this.aiResult = {
                    name: 'AI-generated rule',
                    whenAccount: 'all',
                    whenAmount: '',
                    ifScenario: 'specific_vendor',
                    ifValue: 'from your description',
                    thenAction: 'notify',
                    thenContacts: ['email_1'],
                    thenTransferAccount: '',
                    thenTransferAmount: '',
                    ytdCount: 0,
                    ytdAmount: 0,
                };
            }, 2000);
        },
        saveAiRule(): void {
            if (!this.aiResult) {
                return;
            }

            this.rules.unshift({ ...this.aiResult, id: this.nextRuleId(), enabled: true });
            this.savedMessage = '"' + this.aiResult.name + '" is now active.';
            this.showSaved = true;
            this.aiDescription = '';
            this.aiResult = null;
        },
        editAiResult(): void {
            if (!this.aiResult) {
                return;
            }

            this.draft = {
                whenAccount: [this.aiResult.whenAccount],
                whenAmount: this.aiResult.whenAmount,
                ifScenario: [this.aiResult.ifScenario],
                ifValue: this.aiResult.ifValue,
                thenAction: [this.aiResult.thenAction],
                thenContacts: this.aiResult.thenContacts.slice(),
                thenTransferAccount: [this.aiResult.thenTransferAccount || 'savings'],
                thenTransferAmount: this.aiResult.thenTransferAmount,
            };
            this.builderMode = 'steps';
            this.aiResult = null;
        },
        toggleContact(id: string, checked: boolean): void {
            if (checked && this.draft.thenContacts.indexOf(id) === -1) {
                this.draft.thenContacts.push(id);
            } else if (!checked) {
                this.draft.thenContacts = this.draft.thenContacts.filter((contact: string) => contact !== id);
            }
        },
        saveStepRule(): void {
            if (!this.canSaveStep) {
                return;
            }

            this.rules.unshift({
                id: this.nextRuleId(),
                name: this.labelFor(this.scenarioOptions, this.draft.ifScenario),
                enabled: true,
                whenAccount: firstOf(this.draft.whenAccount),
                whenAmount: this.draft.whenAmount,
                ifScenario: firstOf(this.draft.ifScenario),
                ifValue: this.draft.ifValue,
                thenAction: firstOf(this.draft.thenAction),
                thenContacts: this.draft.thenContacts.slice(),
                thenTransferAccount: firstOf(this.draft.thenTransferAccount),
                thenTransferAmount: this.draft.thenTransferAmount,
                ytdCount: 0,
                ytdAmount: 0,
            });
            this.savedMessage = 'Your new rule is now active.';
            this.showSaved = true;
            this.draft = emptyDraft();
        },
        applyRecipe(recipe: any): void {
            this.draft = {
                whenAccount: recipe.draft.whenAccount.slice(),
                whenAmount: recipe.draft.whenAmount,
                ifScenario: recipe.draft.ifScenario.slice(),
                ifValue: recipe.draft.ifValue,
                thenAction: recipe.draft.thenAction.slice(),
                thenContacts: recipe.draft.thenContacts.slice(),
                thenTransferAccount: recipe.draft.thenTransferAccount.slice(),
                thenTransferAmount: recipe.draft.thenTransferAmount,
            };
            this.builderMode = 'steps';
            document.getElementById('rule-builder')!.scrollIntoView({ behavior: 'smooth' });
        },
        toggleRule(rule: any, enabled: boolean): void {
            rule.enabled = enabled;
        },
        removeRule(id: number): void {
            this.rules = this.rules.filter((rule: any) => rule.id !== id);
        },
        newRule(): void {
            document.getElementById('rule-builder')!.scrollIntoView({ behavior: 'smooth' });
            if (this.builderMode === 'ai') {
                this.$nextTick(() => (this.$refs.aiInput as any).$el.querySelector('input').focus());
            }
        },
        openPanel(type: string): void {
            const titles: { [key: string]: string } = { savings: 'Savings detail', rules: 'Active rules', actions: 'Completed actions' };
            this.slidePanel = { open: true, type, title: titles[type] };
        },
        closePanel(): void {
            this.slidePanel.open = false;
        },
        labelFor(items: any[], value: string[] | string): string {
            const match = items.filter((item: any) => item.value === firstOf(value))[0];
            return match ? match.label : '';
        },
        describeRule(rule: any): string {
            return 'When ' + this.whenText(rule) + ', if ' + this.ifText(rule) + ', then ' + this.thenText(rule);
        },
        whenText(rule: any): string {
            return this.labelFor(this.accountOptions, rule.whenAccount) + (rule.whenAmount ? ' at least $' + rule.whenAmount : '');
        },
        ifText(rule: any): string {
            return this.scenarioText(rule.ifScenario, rule.ifValue);
        },
        thenText(rule: any): string {
            return this.actionText(rule.thenAction, rule);
        },
        scenarioText(scenario: string, value: string): string {
            if (scenario === 'balance_over') { return 'balance over $' + value; }
            if (scenario === 'spent_amount') { return 'spent over $' + value; }
            return 'vendor is "' + value + '"';
        },
        actionText(action: string, rule: any): string {
            if (action === 'transfer') {
                return 'transfer $' + (rule.thenTransferAmount || '?') + ' to ' + this.labelFor(this.savingsAccountOptions, rule.thenTransferAccount);
            }
            const contacts = this.userContacts
                .filter((contact: any) => (rule.thenContacts || []).indexOf(contact.id) !== -1)
                .map((contact: any) => contact.label);
            return 'notify ' + (contacts.length ? contacts.join(' and ') : 'me');
        },
        formatCurrency(value: number): string {
            return value.toFixed(2);
        },
        nextRuleId(): number {
            return this.rules.reduce((maximum: number, rule: any) => Math.max(maximum, rule.id), 0) + 1;
        },
    },
});
</script>
