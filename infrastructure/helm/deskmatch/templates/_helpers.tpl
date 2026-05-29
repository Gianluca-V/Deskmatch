{{/*
Expand the name of the chart.
*/}}
{{- define "deskmatch.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "deskmatch.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "deskmatch.labels" -}}
helm.sh/chart: {{ include "deskmatch.name" . }}-{{ .Chart.Version | replace "+" "_" }}
app.kubernetes.io/name: {{ include "deskmatch.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "deskmatch.selectorLabels" -}}
app.kubernetes.io/name: {{ include "deskmatch.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Service-specific labels
*/}}
{{- define "deskmatch.serviceLabels" -}}
{{ include "deskmatch.labels" . }}
app.kubernetes.io/component: {{ .component }}
{{- end }}

{{/*
Service-specific selector labels
*/}}
{{- define "deskmatch.serviceSelectorLabels" -}}
{{ include "deskmatch.selectorLabels" . }}
app.kubernetes.io/component: {{ .component }}
{{- end }}