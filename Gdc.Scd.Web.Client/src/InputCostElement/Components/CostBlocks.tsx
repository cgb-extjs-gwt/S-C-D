import * as React from 'react';
import { Container, ComboBoxField, Panel, FormPanel, RadioField } from '@extjs/ext-react';
import { CostBlockInputState, EditItem } from '../States/CostBlock'
import { NamedId } from '../../Common/States/NamedId';
import { SelectList, MultiSelectList } from '../../Common/States/SelectList';

export interface CostBlockActions {
  onCountrySelected(countryId: string);
}

export interface SelectListFilter {
  selectList: SelectList<NamedId>
  filter: MultiSelectList<NamedId>,
  filterName: string
}

export interface CostBlockProps {
  country: SelectList<NamedId>,
  costElement: SelectListFilter
  inputLevel: SelectListFilter
  editItems: EditItem[]
}

export class CostBlock extends React.Component<CostBlockProps & CostBlockActions> {
  public render() {
    const { 
      country, 
      costElement: {
        selectList: costElements
      } 
    } = this.props;

    return (
      <Container layout="hbox">
        <Container flex={1} layout="vbox">
          <FormPanel defaults={{labelAlign: 'left'}}>
            {this.countryCombobox(country)}
          </FormPanel>

          <Panel title="Cost Element">
            <FormPanel defaults={{labelAlign: 'left'}} docked="left">
              {
                costElements && 
                costElements.list.map(
                  costElement => this.costElementRadioField(costElement, costElements.selectedItemId))
              }
            </FormPanel>
          </Panel>
        </Container>

        <Container flex={1} layout="vbox">
        </Container>
      </Container>
    );
  }

  private countryCombobox(country: SelectList<NamedId>) {
    const countryStore = Ext.create('Ext.data.Store', {
        data: country.list
    });

    const selectedCountry = 
        countryStore.getData()
                    .findBy(item => (item.data as NamedId).id === country.selectedItemId);

    return (
        <ComboBoxField 
            label="Select a Country:"
            width="50%"
            displayField="name"
            valueField="id"
            queryMode="local"
            store={countryStore}
            selection={selectedCountry}
            onChange={(combobox, newValue, oldValue) => this.props.onCountrySelected(newValue)}
        />
    );
  }

  private costElementRadioField(costElement: NamedId, selectedCostElementId: string) {
    return (
      <RadioField 
          key={costElement.id} 
          boxLabel={costElement.name} 
          name="costElement" 
          checked={costElement.id === selectedCostElementId}
      />
    );
  }
}
