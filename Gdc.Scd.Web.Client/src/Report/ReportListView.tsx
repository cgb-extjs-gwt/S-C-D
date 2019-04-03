import { Container, TreeList } from "@extjs/ext-react";
import * as React from "react";
import { buildComponentUrl } from "../Common/Services/Ajax";

export class ReportListView extends React.Component<any, any> {

    private store = Ext.create('Ext.data.TreeStore', {
        "root": {
            "expanded": true,
            "children": [
                {
                    "text": "<a href='#' data-href='/report/Locap'>1. LOCAP reports (for a specific country)</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/Locap-Detailed'>2. LOCAP reports detailed</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/Contract'>3. Contract reports</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/ProActive-reports'>4. ProActive reports</a>",
                    "leaf": true
                },
                {
                    "text": "5. HDD retention reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/HDD-Retention-country'>a) on country level</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/HDD-Retention-central'>b) as central report</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/HDD-Retention-parameter'>c) HDD retention parameter</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "6. Calculation Parameter Overview reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/Calculation-Parameter-hw'>a) for HW maintenance cost elements (approved)</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/Calculation-Parameter-hw-not-approved'>b) for HW maintenance cost elements (not approved)</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/Calculation-Parameter-proactive'>c) for ProActive cost elements</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "7. Solution Pack reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/SolutionPack-ProActive-Costing'>a) Country SolutionPack ProActive Costing report</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/SolutionPack-Price-List'>b) SolutionPack Price List report</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/SolutionPack-Price-List-Details'>c) SolutionPack Price List Detailed report</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "<a href='#' data-href='/report/PO-Standard-Warranty-Material'>8. PO Standard Warranty Material Report</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/FLAT-Fee-Reports'>9. Availability fee report</a>",
                    "leaf": true
                },
                {
                    "text": "10. Software reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "<a href='#' data-href='/report/SW-Service-Price-List'>a) Software services price list</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/SW-Service-Price-List-detailed'>b) Software services price list detailed</a>",
                            "leaf": true
                        },
                        {
                            "text": "<a href='#' data-href='/report/SW-PARAM-OVERVIEW'>c) Software & Solution calculation parameter overview</a>",
                            "leaf": true
                        }
                    ]
                },
                {
                    "text": "11. Logistics reports",
                    "expanded": true,
                    "children": [
                        {
                            "text": "a) Logistics cost report",
                            "expanded": true,
                            "children": [
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-country'>1. local per country</a>",
                                    "leaf": true
                                }
                            ]
                        },
                        {
                            "text": "b) Logistics cost report input currency",
                            "expanded": true,
                            "children": [
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-input-country'>1. local per country</a>",
                                    "leaf": true
                                }
                            ]
                        },
                        {
                            "text": "c) Calculated logistics cost",
                            "expanded": true,
                            "children": [
                                {
                                    "text": "<a href='#' data-href='/report/Logistic-cost-calc-country'>1. local per country</a>",
                                    "leaf": true
                                }
                            ]
                        }
                    ]
                },
                {
                    "text": "<a href='#' data-href='/report/Locap-Global-Support'>12. LOCAP Global Support Packs(for all the countries)</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/standard-warranty-overview'>13. Standard Warranty overview</a>",
                    "leaf": true
                },
                {
                    "text": "<a href='#' data-href='/report/afr'>14. AFR overview</a>",
                    "leaf": true
                }
            ]
        }
    });

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container padding="25px" scrollable={true}>
                <div onClick={this.onOpenLink}>
                    <TreeList ref="treelist" store={this.store} width="100%" />
                </div>
            </Container>
        );
    }

    private init() {
        this.onOpenLink = this.onOpenLink.bind(this);
    }

    private onOpenLink(e) {

        let target = e.target as HTMLElement;
        let href = target.getAttribute('data-href');

        if (href) {
            href = buildComponentUrl(href);
            this.props.history.push(href);
        }
    }

}