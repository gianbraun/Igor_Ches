using IGOR.Interface.Communication;
using IGOR.Interface.Parameters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace IGOR.Service.Parameters
{
    internal class ParameterService : IParameterService
    {
        private IPlcCom _plcCom;
        private ILogger _log;
        private List<Parameter> _parameters = new List<Parameter>();
        public ParameterService(IPlcCom plcCom, ILogger logger)
        {
            
            _plcCom = plcCom;
            _log = logger.ForContext<ParameterService>();

            _plcCom.PlcStateChanged += (s, e) =>
            {
                if (e == PlcState.Run)
                {
                    WriteAllParametersToPlc();
                    ReadAllParametersFromPlc();
                }
            };

            LoadApplicationParameters();
            LoadPlcParameters();

            CreateGroups();
        }


        #region Interface Implementation

        public object this[string parameterName] => null;

        public IAppParameters AppParameters { get; private set; } = new AppParameters();
        public IPlcParameters PlcParameters { get; private set; } = new PlcParameters();
        public IReadOnlyCollection<ParameterGroup> ParameterGroups { get; private set; }

        public void SaveParameters(IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                parameter.Value = parameter.OnlineValue;
                parameter.OutDated = false;
            }

            SaveAppParameters();
            SavePlcParameters();
        }

        public void SaveParameter(Parameter parameter)
        {
            parameter.Value = parameter.OnlineValue;
            SaveAppParameters();
            SavePlcParameters();
        }

        public void SaveAllOutDateParameters()
        {
            var outDated = _parameters.Where(p => p.OutDated);
            SaveParameters(outDated);
        }

        public void WriteParametersToPlc(IEnumerable<Parameter> plcParameters)
        {
            foreach (var parameter in plcParameters)
            {
                TryWriteParameterToPlc(parameter);
            }

        }

        public bool TryWriteParameterToPlc(Parameter parameter)
        {
            if (parameter is not PlcParameter plcParameter)
                return false;
            var type = Type.GetType(parameter.TypeName);
            var value = Convert.ChangeType(parameter.UpdatedValue, type);

            if (!_plcCom.TryWrite(plcParameter.PlcVariableName, value))
            {
                _log.Error("Parameter \"{paramterName}\" aus der Gruppe \" {parameterGroup} \" kann nicht auf die SPS übertragen werden", parameter.Name, parameter.Group);
                return false;
            }

            return true;
        }

        public bool TryWriteParameterToPlc(Parameter parameter, object value)
        {
            if (parameter is not PlcParameter plcParameter)
                return false;

            if (!_plcCom.TryWrite(plcParameter.PlcVariableName, parameter.Value))
            {
                _log.Error("Parameter \"{paramterName}\" aus der Gruppe \" {parameterGroup} \" kann nicht auf die SPS übertragen werden", parameter.Name, parameter.Group);
                return false;
            }

            return true;
        }

        public void SaveParameters()
        {
            SaveAppParameters();
            SavePlcParameters();
        }

        #endregion

        public void ReadParameterFromPlc(Parameter parameter)
        {
            if (parameter is PlcParameter plcParameter)
            {
                _plcCom.TryRead(plcParameter.PlcVariableName, Type.GetType(parameter.TypeName), out var value);
                parameter.OnlineValue = value;
            }
        }

        public void ReadAllParametersFromPlc()
        {

            foreach (var prop in PlcParameters.GetType().GetProperties())
            {
                if (prop.Name == nameof(PlcParameters.DynamicParameters) || !prop.PropertyType.IsAssignableTo(typeof(PlcParameter)))
                    continue;
                var parameter = (PlcParameter)prop.GetValue(PlcParameters);

                if (parameter == null)
                    continue;

                if (!_plcCom.TryRead(parameter.PlcVariableName, Type.GetType(parameter.TypeName), out var value))
                {
                    _log.Error("Parameter \"{paramterName}\" aus der Gruppe \" {parameterGroup} \" kann nicht gelesen werden", parameter.Name, parameter.Group);
                }
                parameter.OnlineValue = value;
                parameter.Value = value;
            }

            foreach (var parameter in PlcParameters.DynamicParameters)
            {
                _plcCom.TryRead(parameter.PlcVariableName, Type.GetType(parameter.TypeName), out var value);
                parameter.OnlineValue = value;
            }
        }

        private void WriteAllParametersToPlc()
        {
            foreach (var parameter in PlcParameters.DynamicParameters)
            {
                TryWriteParameterToPlc(parameter);
            }

            foreach (var property in PlcParameters.GetType().GetProperties())
            {
                if (!property.PropertyType.IsAssignableTo(typeof(PlcParameter)))
                    continue;

                var parameter = (PlcParameter)property.GetValue(PlcParameters);

                if (parameter == null)
                    continue;

                TryWriteParameterToPlc(parameter);
            }

        }

        private void LoadApplicationParameters()
        {
            var filePath = @"Data\AppParams.json";
            if (!File.Exists(filePath))
            {
                _log.Error("Datei \"{FilePath}\" kann nicht gefunden werden", filePath);
                return;
            }

            var jsonText = File.ReadAllText(filePath);

            try
            {
                var dict = new Dictionary<string, object>();
                var token = JToken.Parse(jsonText);
                foreach (var innertoken in token.Children())
                {
                    var firstinner = innertoken.First;
                    var propName = firstinner.Path;
                    var typeName = firstinner["TypeName"].ToString();
                    var type = Type.GetType(typeName);
                    var paramType = typeof(Parameter<>);
                    var parameter = firstinner.ToObject(paramType.MakeGenericType(type));
                    dict.Add(propName, parameter);
                }

                var parameters = new AppParameters();
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    if (dict.TryGetValue(prop.Name, out var parameter))
                    {
                        prop.SetValue(parameters, parameter);
                        _parameters.Add(parameter as Parameter);
                    }
                    else
                    {
                        _log.Warning("AppParameter {propName} konnte nicht gelesen werden", prop.Name);
                    }
                }

                AppParameters = parameters;

            }
            catch (Exception ex)
            {
                _log.Error(ex, "Laden der App-Parameter fehlgeschlagen");
            }
        }

        private void LoadPlcParameters()
        {
            var filePath = GetPlcParamPath();

            if (!File.Exists(filePath))
            {
                _log.Error("Datei \"{FilePath}\" kann nicht gefunden werden", filePath);
                return;
            }

            var jsonText = File.ReadAllText(filePath);
            List<PlcParameter> dynamicParameters = new();

            var dict = new Dictionary<string, object>();
            var token = JToken.Parse(jsonText);

            try
            {
                foreach (var innertoken in token.Children())
                {
                    var firstinner = innertoken.First;
                    var propName = firstinner.Path;
                    if (propName == "DynamicParameters")
                    {
                        dynamicParameters = GetDynamicParametersFromJson(innertoken);
                        continue;
                    }


                    var typeName = firstinner["TypeName"].ToString();
                    var type = Type.GetType(typeName);
                    var paramType = typeof(PlcParameter<>);
                    var parameter = firstinner.ToObject(paramType.MakeGenericType(type));
                    dict.Add(propName, parameter);
                }


                var parameters = new PlcParameters();
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    if (prop.Name == "DynamicParameters")
                        continue;

                    if (dict.TryGetValue(prop.Name, out var parameter))
                    {
                        _parameters.Add(parameter as Parameter);
                        prop.SetValue(parameters, parameter);
                    }
                    else
                    {
                        _log.Warning("AppParameter {propName} konnte nicht gelesen werden", prop.Name);
                    }
                }

                parameters.DynamicParameters = dynamicParameters;
                _parameters.AddRange(dynamicParameters);
                PlcParameters = parameters;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Fehler beim Parsen der SPS-Parameterdatei aufgetreten");
            }


        }

        private List<PlcParameter> GetDynamicParametersFromJson(JToken token)
        {
            var parameterList = new List<PlcParameter>();
            foreach (var listToken in token.First)
            {
                var typeName = listToken["TypeName"].ToString();
                var type = Type.GetType(typeName);
                var paramType = typeof(PlcParameter<>);
                var parameter = (PlcParameter)listToken.ToObject(paramType.MakeGenericType(type));
                parameterList.Add(parameter);
            }

            return parameterList;
        }

        private void SaveAppParameters()
        {
            var info = new FileInfo(@"Data\AppParams.Json");

            if (!info.Directory.Exists)
                info.Directory.Create();

            try
            {
                var jsonText = JsonConvert.SerializeObject(AppParameters, Formatting.Indented);
                File.WriteAllText(info.FullName, jsonText);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Fehler beim Speichern der Applikations Parameter");
            }
        }

        private string GetPlcParamPath()
             => @"Data\PlcParams.Json";


        private void SavePlcParameters()
        {

            var info = new FileInfo(GetPlcParamPath());

            if (!info.Directory.Exists)
                info.Directory.Create();

            try
            {
                var jsonText = JsonConvert.SerializeObject(PlcParameters, Formatting.Indented);
                File.WriteAllText(info.FullName, jsonText);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Fehler beim Speichern der SPS Parameter");
            }

        }

        private void CreateGroups()
        {
            Dictionary<string, ParameterGroup> paramterGroups = new();

            foreach (var parameter in GetAllParameters())
            {

                var fullGroupName = parameter.Group;
                if (string.IsNullOrWhiteSpace(fullGroupName))
                {
                    // TODO: Lösung für Parameter ohne gruppen
                    continue;
                }

                var groups = fullGroupName.Split("/");

                if (!paramterGroups.TryGetValue(fullGroupName, out var group))
                {
                    group = new ParameterGroup(groups.Last());
                    paramterGroups.Add(parameter.Group, group);
                }
                group.Parameters.Add(parameter);
                fullGroupName = fullGroupName.Replace($"/{groups.Last()}", "");
                var prev = group;
                foreach (var groupName in groups.Reverse().Skip(1))
                {
                    if (!paramterGroups.TryGetValue(fullGroupName, out var parentGroup))
                    {
                        fullGroupName = fullGroupName.Replace($"/{prev.Name}", "");
                        parentGroup = new ParameterGroup(groupName);
                        paramterGroups.Add(fullGroupName, parentGroup);
                    }
                    if (!parentGroup.SubGroups.Contains(prev))
                        parentGroup.SubGroups.Add(prev);

                    prev = parentGroup;
                    fullGroupName = fullGroupName.Replace($"/{groupName}", "");
                }
            }

            ParameterGroups = paramterGroups
                .Where(pair => !pair.Key.Contains("/"))
                .Select(pair => pair.Value).ToList();

        }

        private IEnumerable<Parameter> GetAllParameters()
        {
            foreach (var property in AppParameters.GetType().GetProperties())
            {
                if (!property.PropertyType.IsAssignableTo(typeof(Parameter)))
                    continue;
                var parameter = (Parameter)property.GetValue(AppParameters);
                if (parameter == null)
                    continue;

                yield return parameter;
            }

            foreach (var property in PlcParameters.GetType().GetProperties())
            {
                if (!property.PropertyType.IsAssignableTo(typeof(Parameter)))
                    continue;

                var parameter = (Parameter)property.GetValue(PlcParameters);

                if (parameter == null)
                    continue;

                yield return parameter;
            }

            if (PlcParameters.DynamicParameters.Any())
            {
                foreach (var parameter in PlcParameters.DynamicParameters)
                {
                    yield return parameter;
                }
            }



        }
    }
}
