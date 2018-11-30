import { CheckColumn, Column, Grid, GridCell } from "@extjs/ext-react";
import * as React from "react";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { Model } from "../../Common/States/ExtStates";
import { BundleDetailGroup } from "../States/QualityGateResult";

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
    private readonly titleMap: Map<string, string>;

    private store = Ext.create('Ext.data.Store', {
        fields: [
            'lastInputLevel',
            'coordinates',
            'newValue',
            this.buildNullValueMappingField('oldValue'),
            this.buildNullValueMappingField('countryGroupAvgValue'),
            'isPeriodError',
            'isRegionError',
        ]
    });

    private grid: Grid & any;

    constructor(props: QualityGateGridProps) {
        super(props);

        this.titleMap = this.buildTitleMap(props.costElement);
    }

    public componentDidMount() {
        this.setData();
    }

    public componentWillUpdate() {
        this.setData();
    }

    public render() {
        const { children, hideCheckColumns, inputLevelId, minHeight, flex, onSelectionChange } = this.props;
        const inputLevelName = this.titleMap.get(inputLevelId);

        return (
            <Grid ref={x => this.grid = x} store={this.store} columnLines={true} flex={flex} minHeight={minHeight} onSelectionChange={onSelectionChange}>
                <Column dataIndex="lastInputLevel" text={inputLevelName} renderer={this.rendererWgColumn} align="center" flex={1} />
                <Column dataIndex="coordinates" text="Info" renderer={this.rendererCoordinatesColumn} flex={5} >
                    <GridCell bodyCls="multiline-row" encodeHtml={false} />
                </Column>
                <Column dataIndex="newValue" text="New value" align="center" flex={1} />
                <Column dataIndex="oldValue" text="Old value" align="center" flex={1} />
                <Column dataIndex="countryGroupAvgValue" text="Country group value" flex={1} />
                {
                    !hideCheckColumns &&
                    [
                        <CheckColumn key="isPeriodError" dataIndex="isPeriodError" text="Previous value error" disabled={true} headerCheckbox={false} flex={1} />,
                        <CheckColumn key="isRegionError" dataIndex="isRegionError" text="Quality gate group error" disabled={true} headerCheckbox={false} flex={1} />
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

    private setData() {
        let d = this.props.storeConfig.data;
        this.grid.getStore().setData(d);
    }

}