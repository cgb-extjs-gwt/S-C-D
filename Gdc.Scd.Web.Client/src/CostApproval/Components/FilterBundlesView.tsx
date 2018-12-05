import * as React from 'react';
import { FormPanel, RadioField, CheckBoxField, Panel, Container, DatePickerField, Button } from '@extjs/ext-react';
import { SelectList, NamedId } from '../../Common/States/CommonStates';
import { CostElementId } from '../States/BundleFilterStates';

export interface ApprovalFilterActions{
    onApplicationSelect?(selectedItemId: string)
    onCostBlockCheck?(selectedItemId: string)
    onCostBlockUncheck?(selectedItemId: string)
    onCostElementCheck?(costElementId: string, costBlockId: string)
    onCostElementUncheck?(costElementId: string, costBlockId: string)
    onStartDateChange?(data: Date)
    onEndDateChange?(data: Date)
    onApplyFilter?()
    onShowAllCostBlocksCheck?()
    onShowAllCostCostElementsCheck?()
}

export interface CheckedItem {
    isChecked: boolean
}

export interface CheckedCostBlock extends NamedId, CheckedItem {
}

export interface CheckedCostElement extends CostElementId, CheckedItem {
    name: string
}

export interface FilterApprovalProps extends ApprovalFilterActions{
    application?: SelectList<NamedId>
    costBlocks?: CheckedCostBlock[],
    costElements?: CheckedCostElement[],
    startDate?: Date,
    endDate?: Date,
    isAllCostBlocksChecked?: boolean
    isAllCostElementsChecked?: boolean
}

class Filter extends React.Component<FilterApprovalProps>{
    preventCheckEvents = false
    preventAllCheckEvents = false

    render(){
        if(this.props.application)
        {
            const applicationRadioFields = this.buildApplicationRadioFields();
            const costBlockCheckBoxes = this.buildCostBlockCheckBoxes();
            const costElementsCheckBoxes = this.buildCostElementsCheckBoxes();
            const { onShowAllCostBlocksCheck, onShowAllCostCostElementsCheck, isAllCostBlocksChecked, isAllCostElementsChecked } = this.props;

            return (
                    <FormPanel scrollable shadow
                        layout={{type: 'vbox', align: 'left'}}
                        flex={1}
                        title="Filter By">
                        <h3>Application</h3>
                        { applicationRadioFields }

                        <h3>Cost Blocks</h3>
                        <Container layout={{type: 'hbox'}}>
                            <Panel layout={{type: 'vbox', align: 'left'}}>
                                { this.buildShowAllCheckboks("showAllCostBlocks", isAllCostBlocksChecked, onShowAllCostBlocksCheck) }
                                { costBlockCheckBoxes.slice(0, Math.ceil(costBlockCheckBoxes.length / 2)) }
                            </Panel>
                            <Panel margin='0 15px' layout={{type: 'vbox', align: 'left'}}>
                                { costBlockCheckBoxes.slice(Math.ceil(costBlockCheckBoxes.length / 2), costBlockCheckBoxes.length) }
                            </Panel>
                        </Container> 

                        <h3>Cost Elements</h3>
                        {
                            costElementsCheckBoxes 
                                ? 
                                <Container layout={{type: 'hbox'}}>
                                    <Panel layout={{type: 'vbox', align: 'left'}}>
                                        { this.buildShowAllCheckboks("showAllCostElements", isAllCostElementsChecked, onShowAllCostCostElementsCheck) }
                                        { costElementsCheckBoxes.slice(0, Math.ceil(costElementsCheckBoxes.length / 2)) }
                                    </Panel>
                                    <Panel margin='0 15px' layout={{type: 'vbox', align: 'left'}}>
                                        { costElementsCheckBoxes.slice(Math.ceil(costElementsCheckBoxes.length / 2), costElementsCheckBoxes.length) }
                                    </Panel>
                                </Container> 
                                : 
                                <span style={{color: 'red'}}>
                                    Please select Cost Blocks above...
                                </span>
                        }
                        
                        <h3>Date</h3>
                        <Container layout={{type: 'hbox'}}>
                            <Panel>
                                <DatePickerField 
                                    value={this.props.startDate}
                                    destroyPickerOnHide
                                    dateFormat = "d.m.Y"
                                    label = "From"
                                    picker = {{
                                        yearFrom: 2018
                                    }} 
                                    onChange = {(el, newDate: Date) => this.props.onStartDateChange(newDate)}
                                />
                            </Panel >
                            <Panel margin='0 20px'>
                            <DatePickerField 
                                value={this.props.endDate}
                                destroyPickerOnHide
                                label = "To"
                                dateFormat = "d.m.Y"
                                picker = {{
                                    yearFrom: 2018
                                }} 
                                onChange = {(el, newDate: Date) => this.props.onEndDateChange(newDate)}/>
                            </Panel>
                        </Container>
                        <Container margin="20px 0 0 0" layout={{type: 'hbox', align: 'center'}}>
                                        <Button disabled = { costElementsCheckBoxes ? false : true} 
                                        iconCls="x-fa fa-filter" 
                                        text="Refresh" 
                                        ui = "action raised" 
                                        handler = { this.props.onApplyFilter }
                                        />         
                        </Container>
                    </FormPanel>
            );
        }else
        {
            return (
                <Container masked={{xtype: "loadmask", message: "Loading"}}></Container>
            );
        }
    }

    private buildApplicationRadioFields() {
        const { application, onApplicationSelect } = this.props;

        return application.list.map(item => 
            <RadioField 
                name="application" 
                key={item.id} 
                boxLabel = {item.name}
                value = {item.id}
                checked = {item.id === application.selectedItemId}
                onCheck = {() => onApplicationSelect(item.id)} 
            />
        );
    }

    private buildCostBlockCheckBoxes() {
        const { props, buildCheckChangeFn }  = this;
        const { costBlocks, onCostBlockCheck, onCostBlockUncheck } = props;

        return costBlocks.map(item => 
            <CheckBoxField 
                name="costBlockIds"
                key = {item.id}
                boxLabel = {item.name}
                value = {item.id}
                checked = {item.isChecked}
                onCheck = {buildCheckChangeFn(() => onCostBlockCheck(item.id))}
                onUnCheck = {buildCheckChangeFn(() => onCostBlockUncheck(item.id))} 
            />
        );
    }

    private buildCostElementsCheckBoxes() {
        const { props, buildCheckChangeFn }  = this;
        const { costElements, onCostElementCheck, onCostElementUncheck } = props;

        return costElements.length 
            ? costElements.map(item => 
                <CheckBoxField 
                    name="costElementIds"
                    key = { `${item.costBlockId}_${item.costElementId}` }
                    boxLabel = {item.name}
                    value = {item.costElementId}
                    checked = {item.isChecked}
                    onCheck = {buildCheckChangeFn(() => onCostElementCheck(item.costElementId, item.costBlockId))}
                    onUnCheck = {buildCheckChangeFn(() => onCostElementUncheck(item.costElementId, item.costBlockId))}
                />
            ) 
            : null;
    }

    private buildShowAllCheckboks(name: string, isChecked: boolean, onCheck: (isChecked: boolean) => void) {
        return (
            <CheckBoxField 
                name={name} 
                boxLabel="Show All" 
                cls="checkbox-label-bold"
                checked={isChecked}
                onChange={(checkBox, newValue, oldValue) => {
                    this.preventCheckEvents = true;

                    !this.preventAllCheckEvents && onCheck(newValue)

                    setTimeout(() => {
                        this.preventCheckEvents = false;
                    });
                }}
            />
        );
    }

    private buildCheckChangeFn = (fn: () => void) => () => {
        this.preventAllCheckEvents = true;

        !this.preventCheckEvents && fn();

        setTimeout(() => {
            this.preventAllCheckEvents = false;
        });
    }
}

export default Filter;