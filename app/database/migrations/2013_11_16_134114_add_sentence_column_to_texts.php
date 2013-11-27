<?php

use Illuminate\Database\Migrations\Migration;

class AddSentenceColumnToTexts extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::table('terms', function($table) {
                $table->string('sentence', 500);
            });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::table('terms', function($table) {
                $table->dropColumn('sentence');
            });
    }

}
