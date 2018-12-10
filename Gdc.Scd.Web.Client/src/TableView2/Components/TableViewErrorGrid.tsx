import { CheckColumn, Column, Grid, GridCell, GridProps } from "@extjs/ext-react";
import * as React from "react";
import { WgInputLevel } from "../../Common/Constants/MetaConstants";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { Model } from "../../Common/States/ExtStates";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";

export interface TableViewErrorGridProps extends GridProps {
    costElement: CostElementMeta;
}

export class TableViewErrorGrid extends React.PureComponent<TableViewErrorGridProps> {

    private titleMap: Map<string, string>;

    private inputLevelName: string;

    private grid: Grid & any;

    constructor(props: TableViewErrorGridProps) {
        super(props);
        this.init();
    }

    private init() {
        this.titleMap = this.buildTitleMap(this.props.costElement);
        this.inputLevelName = this.titleMap.get(WgInputLevel);
    }

    public render() {
        return (
            <Grid {...this.props} ref={x => this.grid = x} columnLines={true} >
                <Column dataIndex="lastInputLevel" text={this.inputLevelName} renderer={this.rendererWgColumn} align="center" flex={1} />
                <Column dataIndex="coordinates" text="Info" renderer={this.rendererCoordinatesColumn} flex={5} >
                    <GridCell bodyCls="multiline-row" encodeHtml={false} />
                </Column>
                <Column dataIndex="newValue" text="New value" align="center" flex={1} />
                <Column dataIndex="oldValue" text="Old value" align="center" flex={1} />
                <Column dataIndex="countryGroupAvgValue" text="Country group value" flex={1} />
                <CheckColumn key="isPeriodError" dataIndex="isPeriodError" text="Previous value error" disabled={true} headerCheckbox={false} flex={1} />
                <CheckColumn key="isRegionError" dataIndex="isRegionError" text="Quality gate group error" disabled={true} headerCheckbox={false} flex={1} />
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
}