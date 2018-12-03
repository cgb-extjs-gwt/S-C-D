import * as React from "react";
import { Grid, Column, GridCell, CheckColumn } from "@extjs/ext-react";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { BundleDetailGroup } from "../States/QualityGateResult";
import { Model, Store } from "../../Common/States/ExtStates";

export interface QualityGateGridActions {
    onSelectionChange?(grid, records: any[])
}

export interface QualityGateGridProps extends QualityGateGridActions {
    costElement: CostElementMeta,
    inputLevelId: string
    storeConfig?,
    hideCheckColumns?: boolean
    minHeight?: number | string
    flex?: number
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
        const { children, hideCheckColumns, inputLevelId, minHeight, flex, onSelectionChange } = this.props;
        const inputLevelName = this.titleMap.get(inputLevelId);

        return(
            <Grid store={this.store} columnLines={true} flex={flex} minHeight={minHeight} onSelectionChange={onSelectionChange}>
                <Column dataIndex="lastInputLevel" text={inputLevelName} renderer={this.rendererWgColumn} align="center" flex={1}/>
                <Column dataIndex="coordinates" text="Info" renderer={this.rendererCoordinatesColumn} flex={5} >
                    <GridCell bodyCls="multiline-row" encodeHtml={false}/>
                </Column>
                <Column dataIndex="newValue" text="New value" align="center" flex={1}/>
                <Column dataIndex="oldValue" text="Old value" align="center" flex={1}/>
                <Column dataIndex="countryGroupAvgValue" text="Country group value" flex={1}/>
                {
                    !hideCheckColumns &&
                    [
                        <CheckColumn key="isPeriodError" dataIndex="isPeriodError" text="Previous value error" disabled={true} headerCheckbox={false} flex={1}/>,
                        <CheckColumn key="isRegionError" dataIndex="isRegionError" text="Quality gate group error" disabled={true} headerCheckbox={false} flex={1}/>
                    ]
                }
                
                {children}
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
        const { storeConfig = {} } = this.props;

        return Ext.create('Ext.data.Store', {
            fields: [
                'lastInputLevel', 
                'coordinates',
                'newValue',
                this.buildNullValueMappingField('oldValue'),
                this.buildNullValueMappingField('countryGroupAvgValue'),
                'isPeriodError',
                'isRegionError',
            ],
            ...storeConfig
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

        return result + ' ';
    }

    private rendererWgColumn(value, { data }: Model<BundleDetailGroup>) {
        return data.lastInputLevel.name
    } 

    private buildNullValueMappingField(dataIndex: string) {
        return {
            name: dataIndex,
            mapping: data => {
                const value = data[dataIndex];

                return value == null ? ' ' : value;
            }
        };
    }
}