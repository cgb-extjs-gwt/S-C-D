import { Column, NumberColumn } from "@extjs/ext-react";
import * as React from "react";
import { AutoColumnModel } from "../Model/AutoColumnModel";
import { AutoColumnType } from "../Model/AutoColumnType";
import { IRenderer, moneyRenderer, localMoneyRendererFactory, numberRendererFactory, percentRenderer, stringRenderer, yesNoRenderer } from "./GridRenderer";

const localMoneyRenderer = localMoneyRendererFactory('Currency');

interface reactCmp {
    (key: number): JSX.Element;
}

export class AutoColumnBuilder {

    private m: AutoColumnModel;

    private rndr: IRenderer;

    private factory: reactCmp;

    public constructor(col: AutoColumnModel) {
        this.m = col;
        this.m.flex = col.flex || 1;
        //
        switch (this.m.type) {
            case AutoColumnType.NUMBER:
                this.initNumber();
                break;

            case AutoColumnType.EURO:
                this.initEuro();
                break;

            case AutoColumnType.MONEY:
                this.initMoney();
                break;

            case AutoColumnType.PERCENT:
                this.initPercent();
                break;

            case AutoColumnType.BOOLEAN:
                this.initBoolean();
                break;

            case AutoColumnType.TEXT:
            default:
                this.initTxt();
                break;
        }
    }

    public build(key: number): JSX.Element {
        return this.factory(key);
    }

    private initTxt() {
        this.factory = this.columnCmp;
        this.rndr = stringRenderer;
    }

    private initBoolean() {
        this.factory = this.columnCmp;
        this.rndr = yesNoRenderer;
    }

    private initEuro(): any {
        this.factory = this.columnCmp;
        this.rndr = moneyRenderer;
    }

    private initMoney(): any {
        this.factory = this.columnCmp;
        this.rndr = localMoneyRenderer;
    }

    private initNumber() {
        this.factory = this.numberColumnCmp;
        this.rndr = numberRendererFactory(this.m.format || '0.00###');
    }

    private initPercent() {
        this.factory = this.columnCmp;
        this.rndr = percentRenderer;
    }

    private columnCmp(key: number): JSX.Element {
        let m = this.m;
        return <Column key={key} flex={m.flex} text={m.text} dataIndex={m.name} renderer={this.rndr} />;
    }

    private numberColumnCmp(key: number): JSX.Element {
        let m = this.m;
        return <NumberColumn key={key} flex={m.flex} text={m.text} dataIndex={m.name} renderer={this.rndr} />;
    }
}
