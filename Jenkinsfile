  pipeline {
  agent any
  environment {
    VERSION_PREFIX = "0.1."
    VERSION_NUMBER = "${VERSION_PREFIX}${currentBuild.getNumber()}"
    PACKAGE_NAME = "DigitekAPI"
    PROJECT_WEB = "digitek.brannProsjektering"
    PROJECT_TEST = "digitek.brannProsjektering.Tests"
    OCTOPUS_API_KEY = credentials('arkitektum-octopus-api-key')
    OCTOPUS_PROJECT_NAME = "Digitek API"
    SLACK_CHANNEL = "#feed-digitek"
  }
  stages {
    stage('Build') {
      steps {
        bat "dotnet build --configuration \"Release\" /p:Version=${VERSION_NUMBER} /p:AssemblyVersion=${VERSION_NUMBER}"
      }
    }
    stage('Test') {
      steps {
        bat "dotnet test ${PROJECT_TEST}"
      }
    }
    stage('Build package') {
      steps {
        bat "dotnet publish --configuration Release ${PROJECT_WEB}/${PROJECT_WEB}.csproj --no-build --output output-app"
        dir("${PROJECT_WEB}\\output-app") {
          bat "octo pack --id ${PACKAGE_NAME} --version ${VERSION_NUMBER}"
          bat "octo push --package ${PACKAGE_NAME}.${VERSION_NUMBER}.nupkg --replace-existing --server http://localhost:8081 --apiKey ${OCTOPUS_API_KEY}"
        }
      }
    }
    stage('Deploy') {
      steps {
        bat "octo create-release --project \"${OCTOPUS_PROJECT_NAME}\" --version ${VERSION_NUMBER} --packageversion ${VERSION_NUMBER} --server http://localhost:8081/ --apiKey ${OCTOPUS_API_KEY} --releaseNotes \"Jenkins build [${VERSION_NUMBER}](https://ci.arkitektum.no/blue/organizations/jenkins/${PACKAGE_NAME}/detail/master/${currentBuild.getNumber()}/changes/)\" --deployto=Dev --progress"
      }
    }
  }
  post {
    always {
      dir("${PROJECT_WEB}\\output-app") {
        deleteDir()
      }
      dir("${PROJECT_WEB}\\bin") {
        deleteDir()
      }
      dir("${PROJECT_TEST}\\bin") {
        deleteDir()
      }
    }
    failure {
      slackSend(channel: "${SLACK_CHANNEL}",
        color: 'danger', 
        message: "Build failed: '${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})")
    }
    changed {
      script {
        if (currentBuild.result == null) {
          slackSend(channel: "${SLACK_CHANNEL}",
            color: 'good',
            message: "Build state has changed: 'Success ${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})")
        } else {
          slackSend(channel: "${SLACK_CHANNEL}",
            color: 'warning',
            message: "Build state has changed: '${currentBuild.result} ${env.JOB_NAME} [${env.BUILD_NUMBER}]' (${env.BUILD_URL})")
        }
      }
    }
  }
}
