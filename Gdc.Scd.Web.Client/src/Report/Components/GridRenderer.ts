export const N_A = 'N/A';

export const EUR = 'EUR';

export interface IRenderer {
    (value: any, row: any): string;
}

export function emptyRenderer(val, row) {
    return isEmpty(val) ? '' : val;
}

export function stringRenderer(val, row) {
    return isEmpty(val) ? N_A : val;
}

export function numberRendererFactory(format: string): IRenderer {
    return function (value: any, row: any): string {
        return isEmpty(value) ? N_A : Ext.util.Format.number(value, format);
    }
}

export const numberRenderer: IRenderer = numberRendererFactory('0.00###');

export function moneyRenderer(value: any, row: any): string {
    return currencyRenderer(value, EUR);
}

export function localMoneyRendererFactory(currencyField: string): IRenderer {
    return function (value: any, row: any): string {
        return currencyRenderer(value, row.get(currencyField));
    }
}

export function localToEuroMoneyRendererFactory(exchangeRateField: string): IRenderer {
    return function (value: any, row: any): string {
        return currencyRenderer(value / row.get(exchangeRateField), EUR);
    }
}

export function currencyRenderer(value: any, currency: string): string {
    return isEmpty(value) || isNaN(parseFloat(value)) ? N_A : Ext.util.Format.number(value, '0.00') + ' ' + currency;
}

export function percentRenderer(value: any, row: any): string {
    return isEmpty(value) || isNaN(parseFloat(value)) ? N_A : Ext.util.Format.number(value, '0.00###') + '%';
}

export function yearRenderer(val: number, row) {
    if (isEmpty(val) || val <= 0) {
        return N_A;
    }

    if (val === 1) {
        return val + ' Year';
    }
    else {
        return val + ' Years';
    }
}

export const ddMMyyyyRenderer = Ext.util.Format.dateRenderer('Y-m-d');

export function yesNoRenderer(val, row) {
    return isEmpty(val) ? '' : val ? 'YES' : 'NO';
}

function isEmpty(val: any) {
    return val === null || val === undefined || val === '';
}
