import * as React from "react";
import { Grid, Column, GridCell, CheckColumn, RendererCell } from "@extjs/ext-react";
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
                <Column dataIndex="coordinates" text="Info" flex={5} >
                    <RendererCell 
                        renderer={(value, { data }: Model<BundleDetailGroup>) => 
                            <div style={{whiteSpace: 'normal'}}>
                                {Object.keys(data.coordinates).map(key => this.buildBundleDetail(key, data))}
                            </div>
                        }
                    />
                </Column>
                <Column dataIndex="newValue" text="New value" align="center" flex={1}/>
                {
                    !hideCheckColumns &&
                    [
                        <Column key="oldValue" dataIndex="oldValue" text="Old value" align="center" flex={1} />,
                        <Column key="countryGroupAvgValue" dataIndex="countryGroupAvgValue" text="Quality gate group value" flex={1} />,
                        <CheckColumn key="isPeriodError" dataIndex="isPeriodError" text="Previous value error" disabled={true} headerCheckbox={false} flex={1}/>,
                        <CheckColumn key="isRegionError" dataIndex="isRegionError" text="Quality gate group error" disabled={true} headerCheckbox={false} flex={1}/>
                    ]
                }
                
                {children}
            </Grid>
        );
    }

    private buildBundleDetail(key: string, bundleDetail: BundleDetailGroup) {
        const names = bundleDetail.coordinates[key].map(item => Ext.util.Format.htmlEncode(item.name));
        const title = this.titleMap.get(key);

        return (
            <div style={{paddingBottom: '4px' }}>
                <span style={{fontWeight: 'bold'}}>{title}: </span> 
                {Ext.htmlDecode(names.join(', '))}
            </div>
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
                {
                    name: 'lastInputLevel',
                    sortable: true,
                    sortType: function (value) {
                        return value.name.replace(/(\d+)/g, "0000000000$1").replace(/0*(\d{10,})/g, "$1");
                    },
                },
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