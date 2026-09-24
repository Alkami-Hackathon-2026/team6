import Vue from 'vue';
import MyMoneyRulesApp from './MyMoneyRulesApp.vue';
import '../Styles/money-rules.css';

Vue.config.productionTip = false;

new Vue({
    render: (createElement) => createElement(MyMoneyRulesApp),
}).$mount('#my-money-rules-app');
