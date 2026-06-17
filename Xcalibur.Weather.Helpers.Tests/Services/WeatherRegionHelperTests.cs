using FluentAssertions;
using Xcalibur.Weather.Helpers.Services;

namespace Xcalibur.Weather.Helpers.Tests.Services
{
    /// <summary>
    /// Tests for the <see cref="WeatherRegionHelper"/> class.
    /// </summary>
    public sealed class WeatherRegionHelperTests
    {
        #region DetermineRegion Tests

        [Fact]
        public void DetermineRegion_UnitedStatesContinental_ReturnsUnitedStates()
        {
            // Arrange - New York City
            double lat = 40.7128;
            double lon = -74.0060;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.UnitedStates);
        }

        [Fact]
        public void DetermineRegion_UnitedStatesHawaii_ReturnsUnitedStates()
        {
            // Arrange - Honolulu, Hawaii
            double lat = 21.3099;
            double lon = -157.8581;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.UnitedStates);
        }

        [Fact]
        public void DetermineRegion_UnitedStatesAlaska_ReturnsUnitedStates()
        {
            // Arrange - Anchorage, Alaska
            double lat = 61.2181;
            double lon = -149.9003;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.UnitedStates);
        }

        [Fact]
        public void DetermineRegion_Canada_ReturnsCanada()
        {
            // Arrange - Toronto, Ontario
            double lat = 43.6532;
            double lon = -79.3832;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.Canada);
        }

        [Fact]
        public void DetermineRegion_CanadaVancouver_ReturnsCanada()
        {
            // Arrange - Vancouver, BC
            double lat = 49.2827;
            double lon = -123.1207;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.Canada);
        }

        [Fact]
        public void DetermineRegion_Europe_ReturnsEurope()
        {
            // Arrange - Paris, France
            double lat = 48.8566;
            double lon = 2.3522;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.Europe);
        }

        [Fact]
        public void DetermineRegion_EuropeGermany_ReturnsEurope()
        {
            // Arrange - Berlin, Germany
            double lat = 52.5200;
            double lon = 13.4050;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.Europe);
        }

        [Fact]
        public void DetermineRegion_Australia_ReturnsAustralia()
        {
            // Arrange - Sydney, Australia
            double lat = -33.8688;
            double lon = 151.2093;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.Australia);
        }

        [Fact]
        public void DetermineRegion_AustraliaPerth_ReturnsAustralia()
        {
            // Arrange - Perth, Australia
            double lat = -31.9505;
            double lon = 115.8605;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.Australia);
        }

        [Fact]
        public void DetermineRegion_Other_ReturnsOther()
        {
            // Arrange - Tokyo, Japan (not in covered regions)
            double lat = 35.6762;
            double lon = 139.6503;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.Other);
        }

        [Fact]
        public void DetermineRegion_SouthAmerica_ReturnsOther()
        {
            // Arrange - São Paulo, Brazil
            double lat = -23.5505;
            double lon = -46.6333;

            // Act
            var region = WeatherRegionHelper.DetermineRegion(lat, lon);

            // Assert
            region.Should().Be(WeatherRegion.Other);
        }

        #endregion

        #region IsInGermany Tests

        [Fact]
        public void IsInGermany_Berlin_ReturnsTrue()
        {
            // Arrange - Berlin, Germany
            double lat = 52.5200;
            double lon = 13.4050;

            // Act
            var result = WeatherRegionHelper.IsInGermany(lat, lon);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsInGermany_Munich_ReturnsTrue()
        {
            // Arrange - Munich, Germany
            double lat = 48.1351;
            double lon = 11.5820;

            // Act
            var result = WeatherRegionHelper.IsInGermany(lat, lon);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsInGermany_Hamburg_ReturnsTrue()
        {
            // Arrange - Hamburg, Germany
            double lat = 53.5511;
            double lon = 9.9937;

            // Act
            var result = WeatherRegionHelper.IsInGermany(lat, lon);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsInGermany_Paris_ReturnsFalse()
        {
            // Arrange - Paris, France
            double lat = 48.8566;
            double lon = 2.3522;

            // Act
            var result = WeatherRegionHelper.IsInGermany(lat, lon);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsInGermany_Zurich_ReturnsFalse()
        {
            // Arrange - Zurich, Switzerland
            double lat = 47.3769;
            double lon = 8.5417;

            // Act
            var result = WeatherRegionHelper.IsInGermany(lat, lon);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsInGermany_Vienna_ReturnsFalse()
        {
            // Arrange - Vienna, Austria
            double lat = 48.2082;
            double lon = 16.3738;

            // Act
            var result = WeatherRegionHelper.IsInGermany(lat, lon);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DetermineCanadianProvince Tests

        [Fact]
        public void DetermineCanadianProvince_Toronto_ReturnsON()
        {
            // Arrange - Toronto, Ontario
            double lat = 43.6532;
            double lon = -79.3832;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("on");
        }

        [Fact]
        public void DetermineCanadianProvince_Vancouver_ReturnsBC()
        {
            // Arrange - Vancouver, British Columbia
            double lat = 49.2827;
            double lon = -123.1207;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("bc");
        }

        [Fact]
        public void DetermineCanadianProvince_Montreal_ReturnsQC()
        {
            // Arrange - Montreal, Quebec
            double lat = 45.5017;
            double lon = -73.5673;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("qc");
        }

        [Fact]
        public void DetermineCanadianProvince_Calgary_ReturnsAB()
        {
            // Arrange - Calgary, Alberta
            double lat = 51.0447;
            double lon = -114.0719;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("ab");
        }

        [Fact]
        public void DetermineCanadianProvince_Halifax_ReturnsNS()
        {
            // Arrange - Halifax, Nova Scotia
            double lat = 44.6488;
            double lon = -63.5752;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("ns");
        }

        [Fact]
        public void DetermineCanadianProvince_Winnipeg_ReturnsMB()
        {
            // Arrange - Winnipeg, Manitoba
            double lat = 49.8951;
            double lon = -97.1384;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("mb");
        }

        [Fact]
        public void DetermineCanadianProvince_Regina_ReturnsSK()
        {
            // Arrange - Regina, Saskatchewan
            double lat = 50.4452;
            double lon = -104.6189;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("sk");
        }

        [Fact]
        public void DetermineCanadianProvince_StJohns_ReturnsNL()
        {
            // Arrange - St. John's, Newfoundland and Labrador
            double lat = 47.5615;
            double lon = -52.7126;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("nl");
        }

        [Fact]
        public void DetermineCanadianProvince_Fredericton_ReturnsNB()
        {
            // Arrange - Fredericton, New Brunswick
            double lat = 45.9636;
            double lon = -66.6431;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("nb");
        }

        [Fact]
        public void DetermineCanadianProvince_Charlottetown_ReturnsPE()
        {
            // Arrange - Charlottetown, Prince Edward Island
            double lat = 46.2382;
            double lon = -63.1311;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("pe");
        }

        [Fact]
        public void DetermineCanadianProvince_Yellowknife_ReturnsNT()
        {
            // Arrange - Yellowknife, Northwest Territories
            double lat = 62.4540;
            double lon = -114.3718;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("nt");
        }

        [Fact]
        public void DetermineCanadianProvince_Whitehorse_ReturnsYT()
        {
            // Arrange - Whitehorse, Yukon
            double lat = 60.7212;
            double lon = -135.0568;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("yt");
        }

        [Fact]
        public void DetermineCanadianProvince_Iqaluit_ReturnsNU()
        {
            // Arrange - Iqaluit, Nunavut
            double lat = 63.7467;
            double lon = -68.5170;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().Be("nu");
        }

        [Fact]
        public void DetermineCanadianProvince_NotInCanada_ReturnsNull()
        {
            // Arrange - New York City (not in Canada)
            double lat = 40.7128;
            double lon = -74.0060;

            // Act
            var province = WeatherRegionHelper.DetermineCanadianProvince(lat, lon);

            // Assert
            province.Should().BeNull();
        }

        #endregion

        #region DetermineAustralianState Tests

        [Fact]
        public void DetermineAustralianState_Sydney_ReturnsNSW()
        {
            // Arrange - Sydney, New South Wales
            double lat = -33.8688;
            double lon = 151.2093;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().Be("nsw");
        }

        [Fact]
        public void DetermineAustralianState_Melbourne_ReturnsVIC()
        {
            // Arrange - Melbourne, Victoria
            double lat = -37.8136;
            double lon = 144.9631;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().Be("vic");
        }

        [Fact]
        public void DetermineAustralianState_Brisbane_ReturnsQLD()
        {
            // Arrange - Brisbane, Queensland
            double lat = -27.4698;
            double lon = 153.0251;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().Be("qld");
        }

        [Fact]
        public void DetermineAustralianState_Adelaide_ReturnsSA()
        {
            // Arrange - Adelaide, South Australia
            double lat = -34.9285;
            double lon = 138.6007;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().Be("sa");
        }

        [Fact]
        public void DetermineAustralianState_Perth_ReturnsWA()
        {
            // Arrange - Perth, Western Australia
            double lat = -31.9505;
            double lon = 115.8605;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().Be("wa");
        }

        [Fact]
        public void DetermineAustralianState_Hobart_ReturnsTAS()
        {
            // Arrange - Hobart, Tasmania
            double lat = -42.8821;
            double lon = 147.3272;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().Be("tas");
        }

        [Fact]
        public void DetermineAustralianState_Darwin_ReturnsNT()
        {
            // Arrange - Darwin, Northern Territory
            double lat = -12.4634;
            double lon = 130.8456;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().Be("nt");
        }

        [Fact]
        public void DetermineAustralianState_Canberra_ReturnsACT()
        {
            // Arrange - Canberra, Australian Capital Territory
            double lat = -35.2809;
            double lon = 149.1300;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().Be("act");
        }

        [Fact]
        public void DetermineAustralianState_NotInAustralia_ReturnsNull()
        {
            // Arrange - Auckland, New Zealand (not in Australia)
            double lat = -36.8485;
            double lon = 174.7633;

            // Act
            var state = WeatherRegionHelper.DetermineAustralianState(lat, lon);

            // Assert
            state.Should().BeNull();
        }

        #endregion
    }
}
