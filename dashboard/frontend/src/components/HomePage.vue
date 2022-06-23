<template>
    <div>
        <div class="loading-parent" :style="loaded ? 'display: none' : 'display: flex'">
            <div>loading...</div>
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
                                    <span class="data-summary-key">percentuale di occupazione</span>
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
                    <div class="row" style="padding: 2rem;">
                        <div id="chart" class="chart"></div>
                    </div>
                </div>
                <div class="card">

                </div>
            </div>
            <div class="row">
                <div class="card">
                    <div class="occupance-graphs-section">
                        <div class="occupance-graph">
                            <template v-for="spot in first_floor">
                                <template v-if="spot.Status">
                                    <div class="square taken" :key="spot.Id"></div>
                                </template>
                                <template v-if="!spot.Status">
                                    <div class="square" :key="spot.Id"></div>
                                </template>
                            </template>
                        </div>
                        <div class="occupance-graph">
                            <template v-for="spot in second_floor">
                                <template v-if="spot.Status">
                                    <div class="square taken" :key="spot.Id"></div>
                                </template>
                                <template v-if="!spot.Status">
                                    <div class="square" :key="spot.Id"></div>
                                </template>
                            </template>
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

    .occupance-graphs-section {
        display: flex;
        flex-direction: row;
        justify-content: space-evenly;
        padding: 2rem;
        width: 100%;
    }

    .occupance-graph {
        display: flex;
        flex-wrap: wrap;
        align-content: center;
        width: 100%;
        max-width: 615px;
        margin: 0 1rem;
    }

    .square {
        display: flex;
        border-radius: 8px;
        width: 36px;
        height: 36px;
        margin: 4px;
        background-color: var(--green);
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
            graph_data: null,
            current_rate: null,
            chartTextColor: '#b9b9b9',
            chartFontFamily: 'Inter',
        }
    },
    mounted() {
        // const socket = new WebSocket('ws://localhost:3000')

        // socket.onmessage = ({ data }) => {
        //     console.log(`new message: ${data}`)
        // }

        axios.get('http://localhost:3000/sqltest')
        .then((res) => {
            let data = res.data

            this.first_floor = data[0]
            this.second_floor = data[1]
            this.avg_parking_time = data[2][0].AvgParkingTime
            this.taken_spots = data[3][0].FreeSpotsCount
            this.free_spots = this.total_spots - this.taken_spots
            this.free_spots_percentage = (100 / this.total_spots) * this.taken_spots
            this.graph_data = data[4]
            this.current_rate = data[5][0].CurrentRate

            let dates = []
            let avgTimes = []
            let totalDailyEntries = []

            this.graph_data.forEach(el => {
                dates.push(el.Date)
                avgTimes.push(el.AvgParkingTime)
                totalDailyEntries.push(el.TotalEntries)
            })

            let optionsLine = {
                chart: {
                    type: 'line',
                    height: '100%',
                    width: '100%',
                    foreColor: this.chartTextColor,
                    toolbar: { show: false },
                    zoom: { enabled: false },
                    dropShadow: { enabled: false }
                },
                tooltip: { theme: "dark", style: { fontFamily: this.chartFontFamily } },
                stroke: { curve: 'smooth', width: 2 },
                colors: ["#00F790", '#F7AA00'],
                series: [
                    { name: "permanenza media", data: avgTimes },
                    { name: "ingressi totali", data: totalDailyEntries }
                ],
                markers: { size: 0, strokeWidth: 0, },
                grid: { borderColor: '#3d3d3d' },
                xaxis: {
                    categories: dates,
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
                yaxis: [
                    {
                        forceNiceScale: false,
                        axisTicks: { show: true },
                        axisBorder: {
                            show: true,
                            color: "#FF1654"
                        },
                        labels: { style: { colors: "#FF1654" } },
                        title: {
                            text: "permanenza media",
                            style: { color: "#FF1654" }
                        }
                    },
                    {
                        forceNiceScale: false,
                        opposite: true,
                        axisTicks: { show: true },
                        axisBorder: {
                            show: true,
                            color: "#247BA0"
                        },
                        labels: { style: { colors: "#247BA0" } },
                        title: {
                            text: "ingressi totali",
                            style: { color: "#247BA0" }
                        }
                    }
                ],
                legend: { position: 'bottom', horizontalAlign: 'center', fontFamily: this.chartFontFamily },
                responsive: [
                    {
                        breakpoint: 1000,
                        options: {
                        }
                    }
                ]
            }

            let chartLine = new ApexCharts(document.querySelector('#chart'), optionsLine);
            chartLine.render();

            this.loaded = true 
        })
    }
}
</script>

