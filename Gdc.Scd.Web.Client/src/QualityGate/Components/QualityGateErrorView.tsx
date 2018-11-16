import * as React from "react";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { Container, FormPanel, TextField, Toolbar, Button, Grid, Column, GridCell, CheckColumn } from "@extjs/ext-react";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { BundleDetailGroup } from "../States/QualityGateResult";
import { Model } from "../../Common/States/ExtStates";
import { CostElementMeta } from "../../Common/States/CostMetaStates";

export interface QualityGateErrorActions {
    onSave?(explanationMessage: string)
    onCancel?()
}

export interface QualityGateErrorProps extends QualityGateErrorActions {
    errors?: BundleDetailGroup[]
    costElement: CostElementMeta
}

interface QualityGateErrorState {
    isValidExplanationForm: boolean
}

export class QualityGateErrorView extends React.Component<QualityGateErrorProps, QualityGateErrorState> {
    private readonly titleMap: Map<string, string>;
    private explanationTextField
    private explanationForm

    constructor(props: QualityGateErrorProps){ 
        super(props);

        this.state = {
            isValidExplanationForm: false
        };

        this.titleMap = this.buildTitleMap(props.costElement);
    }

    render() {
        const { onCancel } = this.props;
        const { isValidExplanationForm } = this.state;
        const store = this.buildStore();

        return (
            <Container layout="vbox" scrollable={true}>
                <Grid store={store} columnLines={true} flex={10}>
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
                
                <FormPanel defaults={{labelAlign: 'left'}} flex={1} ref={form => this.explanationForm = form} minHeight="100">
                    <TextField 
                        ref={textField => this.explanationTextField = textField}
                        required
                        placeholder="Please enter the reason"
                        onChange={this.onExplanationTextFieldChange}
                    />
                    <Toolbar docked="bottom">
                        <Button 
                            text="Save" 
                            handler={this.onSave} 
                            flex={1} 
                            disabled={!isValidExplanationForm}
                        />
                        <Button text="Cancel" handler={onCancel} flex={1}/>
                    </Toolbar>
                </FormPanel>
            </Container>
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

    private onExplanationTextFieldChange = (textField, newValue: string, oldValue: string) => {
        this.setState({ isValidExplanationForm: this.explanationForm.isValid() });
    }

    private onSave = () => {
        const { onSave } = this.props;

        onSave && onSave(this.explanationTextField.getValue());
    }
}