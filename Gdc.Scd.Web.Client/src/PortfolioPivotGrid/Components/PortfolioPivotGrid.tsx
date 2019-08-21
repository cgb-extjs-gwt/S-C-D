import * as React from 'react';
import { Container } from '@extjs/ext-react';
import { PivotGrid } from '@extjs/ext-react-pivot';
import { buildMvcUrl } from '../../Common/Services/Ajax';

export class PortfolioPivotGrid extends React.Component {
    render() {
        return (
            <Container layout="fit">
                <PivotGrid matrix={this.getMatrix()} store={null}>
                </PivotGrid>
            </Container>
        );
    }

    private getMatrix() {
        return {
            type: 'remote',
            url: buildMvcUrl('PortfolioPivotGrid', 'GetData'),
            timeout: 600000,
            rowGrandTotalsPosition: 'none',
            colGrandTotalsPosition: 'none',
            colSubTotalsPosition: 'none',
            aggregate: [{
                id: 'count',
                dataIndex: 'count',
                header: 'Count',
                aggregator: 'count',
                renderer: aggregateRenderer
            }],
            leftAxis: [
                {
                    id: 'ServiceLocation',
                    dataIndex: 'ServiceLocationId',
                    header: 'Service location',
                    renderer: axisRenderer
                },
                {
                    id: 'ReactionTime',
                    dataIndex: 'ReactionTimeId',
                    header: 'Reaction time',
                    renderer: axisRenderer
                },
                {
                    id: 'ReactionType',
                    dataIndex: 'ReactionTypeId',
                    header: 'Reaction type',
                    renderer: axisRenderer
                },
                {
                    id: 'Availability',
                    dataIndex: 'AvailabilityId',
                    header: 'Availability',
                    renderer: axisRenderer
                },
                {
                    id: 'ProActive',
                    dataIndex: 'ProActiveSlaId',
                    header: 'ProActive',
                    renderer: axisRenderer
                },
            ],
            topAxis: [
                {
                    id: 'Sog',
                    dataIndex: 'SogId',
                    header: 'Sog',
                    renderer: axisRenderer
                },
                {
                    id: 'Wg',
                    dataIndex: 'WgId',
                    header: 'Wg',
                    renderer: axisRenderer
                },
            ]
        };

        function axisRenderer(value) {
            return value == null ? ' ' : value;
        }

        function aggregateRenderer(value) {
            return value == null ? 0 : value;
        }
    }
}