pipeline {
    agent any

    options {
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    stages {
        stage('Build') {
            steps {
                sh 'rm -rf test-results'
                sh 'dotnet build CinemaBooking.slnx -c Release'
            }
        }

        stage('Unit tests') {
            steps {
                sh 'dotnet test CinemaBooking.slnx -c Release --no-build --filter "Category!=Integration" --logger trx --results-directory test-results/unit'
            }
        }

        stage('Integration tests') {
            steps {
                sh 'dotnet test CinemaBooking.slnx -c Release --no-build --filter "Category=Integration" --logger trx --results-directory test-results/integration'
            }
        }

        stage('Publish images') {
            steps {
                sh '''
                    for service in Identity Catalog Bookings; do
                        image=cinema-$(echo $service | tr '[:upper:]' '[:lower:]')
                        docker build --build-arg SERVICE=$service -t $image:$BUILD_NUMBER -t $image:latest .
                    done
                '''
            }
        }
    }

    post {
        always {
            mstest testResultsFile: 'test-results/**/*.trx', failOnError: false
        }
    }
}
