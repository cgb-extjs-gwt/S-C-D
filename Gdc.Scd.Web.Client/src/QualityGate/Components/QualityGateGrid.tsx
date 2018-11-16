import * as React from "react";
import { Grid, Column, GridCell, CheckColumn } from "@extjs/ext-react";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { BundleDetailGroup } from "../States/QualityGateResult";
import { Model, Store } from "../../Common/States/ExtStates";

export interface QualityGateGridProps {
    errors?: BundleDetailGroup[]
    costElement: CostElementMeta
}

export class QualityGateGrid extends React.PureComponent<QualityGateGridProps> {
    private readonly titleMap: Map<string, string>
    private readonly store: Store

    constructor(props: QualityGateGridProps) {
        super(props);

        this.titleMap = this.buildTitleMap(props.costElement);
        this.store = this.buildStore();
    }

    public render() {
        return(
            <Grid store={this.store} columnLines={true} flex={10}>
                <Column dataIndex="wg" text="Wg" renderer={this.rendererWgColumn} align="center" flex={1}/>
                <Column dataIndex="coordinates" text="Info" renderer={this.rendererCoordinatesColumn} flex={5} >
                    <GridCell bodyCls="multiline-row" encodeHtml={false}/>
                </Column>
                <Column dataIndex="newValue" text="New value" align="center" flex={1}/>
                <Column dataIndex="oldValue" text="Old value" align="center" flex={1}/>
                <Column dataIndex="countryGroupAvgValue" text="Country group value" flex={1}/>
                <CheckColumn dataIndex="isPeriodError" text="Previous value error" disabled={true} headerCheckbox={false} flex={1}/>
                <CheckColumn dataIndex="isRegionError" text="Quality gate group error" disabled={true} headerCheckbox={false} flex={1}/>
            </Grid>
        );
    }

    private buildTitleMap({ dependency, inputLevels }: CostElementMeta) {
        const titleMap = new Map<string, string>();

        if (dependency) {
            titleMap.set(dependency.id, dependency.name);
        }

        for (const inputLevel of inputLevels) {
            titleMap.set(inputLevel.id, inputLevel.name);
        }

        return titleMap;
    }

    private buildStore() {
        const { errors } = this.props;

        return Ext.create('Ext.data.Store', {
            fields: [
                'lastInputLevel', 
                'coordinates',
                'newValue',
                'oldValue',
                { 
                    name: 'countryGroupAvgValue', 
                    mapping: (data: BundleDetailGroup) => data.countryGroupAvgValue == null ? ' ' : data.countryGroupAvgValue
                },
                'isPeriodError',
                'isRegionError',
            ],
            data: errors
        });
    }

    private rendererCoordinatesColumn = (value, { data }: Model<BundleDetailGroup>) => {
        let result = '';

        for (const key of Object.keys(data.coordinates)) {
            const names = data.coordinates[key].map(item => Ext.util.Format.htmlEncode(item.name));
            const title = this.titleMap.get(key);

            result += `
                <div style="padding-bottom: 4px;">
                    <span style="font-weight: bold;">${title}:</span> ${names.join(', ')}
                </div>`
        }

        return result;
    }

    private rendererWgColumn(value, { data }: Model<BundleDetailGroup>) {
        return data.wg.name
    } 
}