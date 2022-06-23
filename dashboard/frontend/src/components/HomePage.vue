<template>
    <div class="loading-parent">
        <div class="loading-parent" :style="loaded ? 'display: none' : 'display: flex'">
            <div style="font-size: 1.5rem">loading...</div>
        </div>
        <div :style="loaded ? 'display: block' : 'display: none'">
            <div class="row">
                <div class="card" style="justify-content: space-between; align-items: center">
                    <div class="col data-summary">
                        <div class="row">
                            <div class="col">
                                <div class="row">
                                    <span class="data-summary-key">posti occupati</span>
                                </div>
                                <div class="row">
                                    <span class="data-summary-value" style="color: var(--orange)">{{taken_spots}}</span>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col">
                                <div class="row">
                                    <span class="data-summary-key">posti liberi</span>
                                </div>
                                <div class="row">
                                    <span class="data-summary-value" style="color: var(--green)">{{free_spots}}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="divider"></div>
                    <div class="col data-summary">
                        <div class="row">
                            <div class="col">
                                <div class="row">
                                    <span class="data-summary-key">occupazione</span>
                                </div>
                                <div class="row">
                                    <span class="data-summary-value">{{free_spots_percentage}}%</span>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col">
                                <div class="row">
                                    <span class="data-summary-key">tariffa attuale</span>
                                </div>
                                <div class="row">
                                    <span class="data-summary-value">{{current_rate}}€</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="card">
                    <div class="row chart-parent">
                        <span>permanenza media</span>
                        <div id="avg_time_chart" class="chart"></div>
                    </div>
                </div>
                <div class="card">
                    <div class="row chart-parent">
                        <span>ingressi totali</span>
                        <div id="total_entries_chart" class="chart"></div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="card" style="flex-direction: column; justify-content: center;">
                    <div class="row">
                        <div style="margin: 2rem 0 0 6rem">occupazione attuale</div>
                    </div>
                    <div class="row">
                        <div class="occupance-graphs-section">
                            <div class="occupance-graph">
                                <span class="floor-label">piano 0</span>
                                <div class="spots">
                                    <template v-for="spot in first_floor">
                                        <template v-if="spot.Status">
                                            <div class="square taken" :key="spot.Id" @mouseover="showTip(spot.Id)" @mouseleave="hideTip(spot.Id)" :id="spot.Id">
                                                <div class="square-tip" :id="'tip_'+spot.Id">{{spot.Id}}</div>
                                            </div>
                                        </template>
                                        <template v-if="!spot.Status">
                                            <div class="square" :key="spot.Id" @mouseover="showTip(spot.Id)" @mouseleave="hideTip(spot.Id)" :id="spot.Id">
                                                <div class="square-tip" :id="'tip_'+spot.Id">{{spot.Id}}</div>
                                            </div>
                                        </template>
                                    </template>
                                </div>
                            </div>
                            <div class="occupance-graph">
                                <span class="floor-label">piano 1</span>
                                <div class="spots">
                                    <template v-for="spot in second_floor">
                                        <template v-if="spot.Status">
                                            <div class="square taken" :key="spot.Id" @mouseover="showTip(spot.Id)" @mouseleave="hideTip(spot.Id)" :id="spot.Id">
                                                <div class="square-tip" :id="'tip_'+spot.Id">{{spot.Id}}</div>
                                            </div>
                                        </template>
                                        <template v-if="!spot.Status">
                                            <div class="square" :key="spot.Id" @mouseover="showTip(spot.Id)" @mouseleave="hideTip(spot.Id)" :id="spot.Id">
                                                <div class="square-tip" :id="'tip_'+spot.Id">{{spot.Id}}</div>
                                            </div>
                                        </template>
                                    </template>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
    .row {
        flex-wrap: wrap;
    }

    .col {
        display: flex;
        flex-direction: column;
        flex: 1
    }

    .data-summary {
        padding: 2rem;
        justify-content: space-evenly;
        height: 100%
    }

    .data-summary .row .col .row {
        justify-content: center;
    }

    .data-summary-key {
        font-size: 1.1rem
    }

    .data-summary-value {
        font-size: 3rem
    }

    .divider {
        height: 70%;
        width: 1px;
        background-color: rgba(255,255,255,0.7);
    }

    .chart-parent {
        padding: 2rem; 
        align-items: flex-start;
        flex-wrap: nowrap;
        flex-direction: column;
    }

    .chart-parent > span {
        margin-bottom: 1rem;
    }

    .occupance-graphs-section {
        display: flex;
        flex-direction: row;
        justify-content: space-evenly;
        padding: 2rem;
        margin-bottom: 2rem;
        width: 100%;
    }

    .occupance-graph {
        display: flex;
        flex-wrap: wrap;
        align-content: center;
        justify-content: center;
        width: 100%;
        max-width: 615px;
        margin: 0 1rem;
    }

    .floor-label {
        font-size: 1.3rem;
        margin-bottom: 2rem;
    }

    .spots {
        display: flex;
        flex-wrap: wrap;
    }

    .square {
        display: flex;
        border-radius: 8px;
        width: 36px;
        height: 36px;
        margin: 4px;
        background-color: var(--green);
        position: relative
    }

    .square:hover {
        background-color: var(--green-hover);
    }

    .taken {
        background-color: var(--orange);
    }

    .taken:hover {
        background-color: var(--orange-hover)
    }

    .square-tip {
        width: 50px;
        height: 28px;
        border-radius: 6px;
        background-color: black;
        top: -30px;
        left: -7px;
        color: white;
        position: absolute;
        display: none;
        justify-content: center;
        align-items: center;
    }

    .loading-parent {
        width: 100%;
        height: 100%;
        display: flex;
        justify-content: center;
        align-items: center;
    }

    @media screen and (max-width: 1040px) {
        .occupance-graphs-section {
            flex-direction: column;
        }

        .occupance-graph {
            margin: 1rem 0;
            justify-content: center;
        }
    }
</style>

<script>
import ApexCharts from 'apexcharts'
import axios from 'axios'

export default {
    name: 'HomePage',
    data() {
        return {
            total_spots: 100,
            first_floor: null,
            second_floor: null,
            loaded: false,
            free_spots: null,
            taken_spots: null,
            avg_parking_time: null,
            free_spots_percentage: null,
            one_week_ago_graph_data: null,
            two_weeks_ago_graph_data: null,
            current_rate: null,
            chartTextColor: '#b9b9b9',
            chartFontFamily: 'Inter',
        }
    },
    mounted() {
        const socket = new WebSocket('ws://localhost:3000')

        socket.onmessage = ({ data }) => {
            try {
                let message = JSON.parse(data)

                if(Object.hasOwn(message, 'taken')) {
                    if(parseInt(message._id) < 60) {
                        let updateIndex = this.first_floor.findIndex(s => {
                            return s.Id === message._id
                        })
                        this.first_floor[updateIndex].Status = message.taken
                    } else {
                        let idx = this.second_floor.findIndex(s => {
                            return s.Id === message._id
                        })
                        this.second_floor[idx].Status = message.taken
                    }
                    this.updateCounts()
                }
            } catch(err) {
                console.log(err)
            }
        }

        axios.get('http://localhost:3000/getData')
        .then((res) => {
            let data = res.data

            this.first_floor = data[0]
            this.second_floor = data[1]
            this.avg_parking_time = data[2][0].AvgParkingTime
            this.taken_spots = this.countTakenSpots()
            this.free_spots = this.total_spots - this.taken_spots
            this.free_spots_percentage = (100 / this.total_spots) * this.taken_spots
            this.one_week_ago_graph_data = data[4]
            this.current_rate = data[5][0].CurrentRate
            this.two_weeks_ago_graph_data = data[6]

            let dates = [[],[]]
            let avgTime = [[],[]]
            let totalEntries = [[],[]]

            this.one_week_ago_graph_data.forEach(el => {
                dates[0].push(el.Date)
                avgTime[0].push(el.AvgParkingTime)
                totalEntries[0].push(el.TotalEntries)
            })

            this.two_weeks_ago_graph_data.forEach(el => {
                dates[1].push(el.Date)
                avgTime[1].push(el.AvgParkingTime)
                totalEntries[1].push(el.TotalEntries)
            })

            let avgTimeChartOptions = {
                chart: {
                    type: 'line',
                    height: '90%',
                    width: '100%',
                    foreColor: this.chartTextColor,
                    toolbar: { show: false },
                    zoom: { enabled: false },
                    dropShadow: { enabled: false }
                },
                tooltip: { 
                    theme: "dark", 
                    style: { fontFamily: this.chartFontFamily },
                    x: {
                        formatter: () => {
                            return 'permanenza media'
                        }
                    },
                    y: {
                        title: {
                            formatter: () => { return '' }
                        }
                    }
                },
                stroke: { curve: 'smooth', width: 2 },
                colors: ["#00F790", '#F7AA00'],
                series: [
                    { name: "ultima settimana", data: avgTime[0] },
                    { name: "settimana precedente", data: avgTime[1] }
                ],
                markers: { size: 0, strokeWidth: 0, },
                grid: { borderColor: '#3d3d3d' },
                xaxis: {
                    categories: [1,2,3,4,5,6,7],
                    type: 'datetime',
                    tooltip: { enabled: false },
                    labels: {
                        style: { 
                            colors: this.chartTextColor, 
                            fontFamily: this.chartFontFamily 
                        },
                        datetimeUTC: true,
                        format: 'dd/MM/yyyy'
                    },
                },
                yaxis: {
                    forceNiceScale: true,
                    labels: { style: { colors: [this.chartTextColor], fontFamily: this.chartFontFamily } }
                },
                legend: { position: 'bottom', horizontalAlign: 'center', fontFamily: this.chartFontFamily },
                responsive: [
                    {
                        breakpoint: 1000,
                        options: {
                            chart: {
                                width: '90%'
                            }
                        }
                    }
                ]
            }

            let avgTimeChart = new ApexCharts(document.querySelector('#avg_time_chart'), avgTimeChartOptions);

            let totalEntriesChartOptions = {
                chart: {
                    type: 'line',
                    height: '90%',
                    width: '100%',
                    foreColor: this.chartTextColor,
                    toolbar: { show: false },
                    zoom: { enabled: false },
                    dropShadow: { enabled: false }
                },
                tooltip: { 
                    theme: "dark", 
                    style: { fontFamily: this.chartFontFamily },
                    x: {
                        formatter: () => {
                            return 'ingressi totali'
                        }
                    },
                    y: {
                        title: {
                            formatter: () => { return '' }
                        }
                    }
                },
                stroke: { curve: 'smooth', width: 2 },
                colors: ["#00F790", '#F7AA00'],
                series: [
                    { name: 'ultima settimana', data: totalEntries[0] },
                    { name: 'settimana precedente', data: totalEntries[1] }
                ],
                markers: { size: 0, strokeWidth: 0, },
                grid: { borderColor: '#3d3d3d' },
                xaxis: {
                    categories: [1,2,3,4,5,6,7],
                    type: 'datetime',
                    tooltip: { enabled: false },
                    labels: {
                        style: { 
                            colors: this.chartTextColor, 
                            fontFamily: this.chartFontFamily 
                        },
                        datetimeUTC: true,
                        format: 'dd/MM/yyyy'
                    },
                },
                yaxis: {
                    forceNiceScale: true,
                    labels: { style: { colors: [this.chartTextColor], fontFamily: this.chartFontFamily } }
                },
                legend: { position: 'bottom', horizontalAlign: 'center', fontFamily: this.chartFontFamily },
                responsive: [
                    {
                        breakpoint: 1000,
                        options: {
                            chart: {
                                width: '90%'
                            }
                        }
                    }
                ]
            }

            let totalEntriesChart = new ApexCharts(document.querySelector('#total_entries_chart'), totalEntriesChartOptions);

            avgTimeChart.render();
            totalEntriesChart.render()

            this.loaded = true 
        })
    },
    methods: {
        showTip: (id) => {
            document.getElementById(`tip_${id}`).style.display = 'flex'
        },
        hideTip: (id) => {
            document.getElementById(`tip_${id}`).style.display = 'none'
        },
        countTakenSpots() {
            let count = 0
            this.first_floor.forEach(spot => {
                if(spot.Status)
                    count++
            })
            this.second_floor.forEach(spot => {
                if(spot.Status)
                    count++
            })

            return count
        }, 
        updateCounts() {
            this.taken_spots = this.countTakenSpots()
            this.free_spots = this.total_spots - this.taken_spots
            this.free_spots_percentage = (100 / this.total_spots) * this.taken_spots
        }
    }
}
</script>

