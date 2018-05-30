import * as React from 'react';
import { Container, ComboBoxField, Panel } from '@extjs/ext-react';
import { CostBlockInputState } from '../States/CostBlock'
import { NamedId } from '../../Common/States/NamedId';

export interface CostBlockDispatch {
  onCountrySelected(countryId: string);
}

export interface CostBlockProps extends CostBlockDispatch {
  costBlock: CostBlockInputState,
  countries: NamedId[]
}

export class CostBlock extends React.Component<CostBlockProps, CostBlockDispatch> {
  public render() {
    const { costBlock, countries } = this.props;

    return (
      <Container>
        <Container flex={1} layout="vbox" docked="left">
          {this.countryCombobox(countries, costBlock.selectedCountryId)}

          <Panel title="Cost Element">

          </Panel>
        </Container>

        <Container flex={1} layout="vbox" docked="right">
        </Container>
      </Container>
    );
  }

  private countryCombobox(contries: NamedId[], selectedCountryId: string) {
    const countryStore = Ext.create('Ext.data.Store', {
      data: contries
  });

  const selectedCountry = 
      countryStore.getData()
                  .findBy(item => (item.data as NamedId).id === selectedCountryId);

  return (
      <ComboBoxField 
          label="Select a Country:"
          width="25%"
          displayField="name"
          valueField="id"
          queryMode="local"
          store={countryStore}
          selection={selectedCountry}
          onChange={(combobox, newValue, oldValue) => this.props.onCountrySelected(newValue)}
      />
  );
  }
}
